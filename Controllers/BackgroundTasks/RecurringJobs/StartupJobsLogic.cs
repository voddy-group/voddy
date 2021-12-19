using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using Quartz;
using Quartz.Impl;
using RestSharp;
using voddy.Controllers.BackgroundTasks.LiveStreamDownloads;
using voddy.Controllers.BackgroundTasks.LiveStreamDownloads.LiveStreamDownloadJobs;
using voddy.Controllers.BackgroundTasks.StreamDownloads.StreamDownloadJobs;
using voddy.Controllers.Database;
using voddy.Controllers.Notifications;
using voddy.Controllers.Structures;
using voddy.Controllers.Setup.Update;
using voddy.Databases.Chat;
using voddy.Databases.Logs;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;
using Stream = voddy.Databases.Main.Models.Stream;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs {
    public class StartupJobsLogic {
        private Logger Logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();


        public void StreamerCheckForUpdates() {
            Notification notification = NotificationLogic.CreateNotification("streamerCheckForUpdates", Severity.Info, Position.General, "Checking for streamer updates...");
            List<Streamer> listOfStreamers = new List<Streamer>();
            using (var context = new MainDataContext()) {
                listOfStreamers = context.Streamers.ToList();
            }

            if (listOfStreamers.Count > 100) {
                for (int i = 0; i < listOfStreamers.Count; i = i + 100) {
                    UpdateStreamerDetails(listOfStreamers.Skip(i).Take(100).ToList());
                }
            } else {
                UpdateStreamerDetails(listOfStreamers);
            }

            NotificationLogic.DeleteNotification("streamerCheckForUpdates");
        }

        public void UpdateStreamerDetails(List<Streamer> listOfStreamers) {
            string listOfIds = "?id=";
            for (int i = 0; i < listOfStreamers.Count; i++) {
                if (i != listOfStreamers.Count - 1) {
                    listOfIds += listOfStreamers[i].streamerId + "&id=";
                } else {
                    listOfIds += listOfStreamers[i].streamerId;
                }
            }

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest($"https://api.twitch.tv/helix/users{listOfIds}", Method.GET);
            var deserializedResponse = JsonConvert.DeserializeObject<UserJsonClass.User>(response.Content);


            for (int i = 0; i < deserializedResponse.data.Count; i++) {
                Streamer result = new Streamer {
                    streamerId = deserializedResponse.data[i].id,
                    displayName = deserializedResponse.data[i].display_name,
                    username = deserializedResponse.data[i].login,
                    thumbnailLocation = deserializedResponse.data[i].profile_image_url,
                    description = deserializedResponse.data[i].description,
                    viewCount = deserializedResponse.data[i].view_count,
                };

                StreamerLogic streamerLogic = new StreamerLogic();
                streamerLogic.UpdateStreamer(result, null);
            }
        }


        public void CheckForStreamerLiveStatus() {
            Notification notification = NotificationLogic.CreateNotification("checkForLiveStreamStatus", Severity.Info, Position.General, "Checking for live streamers...");
            Logger.Info("Checking for live streams to download...");
            List<Streamer> listOfStreamers = new List<Streamer>();
            using (var context = new MainDataContext()) {
                listOfStreamers = context.Streamers.ToList(); //.Where(item => item.getLive).ToList();
            }

            if (listOfStreamers.Count > 100) {
                for (int i = 0; i < listOfStreamers.Count; i = i + 100) {
                    UpdateLiveStatus(listOfStreamers.Skip(i).Take(100).ToList());
                }
            } else {
                UpdateLiveStatus(listOfStreamers);
            }

            Logger.Info("Done!");
            NotificationLogic.DeleteNotification("checkForLiveStreamStatus");
        }


        public void CheckStreamFileExists() {
            var checkFiles = new CheckFiles();
        }

        public void UpdateLiveStatus(List<Streamer> listOfStreamers) {
            string listOfIds = "?user_id=";
            for (int i = 0; i < listOfStreamers.Count; i++) {
                if (i != listOfStreamers.Count - 1) {
                    listOfIds += listOfStreamers[i].streamerId + "&user_id=";
                } else {
                    listOfIds += listOfStreamers[i].streamerId;
                }
            }

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest($"https://api.twitch.tv/helix/streams{listOfIds}&first=100",
                Method.GET);
            StreamHelpers.GetStreamsResult liveStream =
                JsonConvert.DeserializeObject<StreamHelpers.GetStreamsResult>(response.Content);

            for (int x = 0; x < listOfStreamers.Count; x++) {
                var stream = liveStream.data.FirstOrDefault(item => item.user_id == listOfStreamers[x].streamerId);

                if (stream != null && stream.type == "live") {
                    // if live and if not a re-run or something else

                    using (var context = new MainDataContext()) {
                        var alreadyExistingStream =
                            context.Streams.FirstOrDefault(item => item.vodId == Int64.Parse(stream.id));

                        var streamer =
                            context.Streamers.FirstOrDefault(item => item.streamerId == listOfStreamers[x].streamerId);

                        if (streamer.isLive == false) {
                            streamer.isLive = true;
                            context.SaveChanges();
                        }
                        
                        NotificationHub.Current.Clients.All.SendAsync($"{streamer.id}Live",
                            true);

                        if (streamer.getLive == false || alreadyExistingStream != null) {
                            // already downloading/downloaded, or user does not want to download this streamers live stream
                            continue;
                        }
                    }

                    if (DateTime.UtcNow.Subtract(stream.started_at).TotalMinutes < 5) {
                        // queue up the stream to be downloaded
                        StreamExtended convertedLiveStream = new StreamExtended {
                            streamId = StreamHelpers.GetStreamDetails(Int64.Parse(stream.id), true, stream.user_id).streamId,
                            vodId = Int64.Parse(stream.id),
                            streamerId = stream.user_id,
                            title = stream.title,
                            createdAt = stream.started_at
                        };

                        CreateLiveStream createLiveStream = new CreateLiveStream();
                        createLiveStream.PrepareLiveStreamDownload(convertedLiveStream, stream.user_login);
                    }
                } else {
                    using (var context = new MainDataContext()) {
                        var streamer =
                            context.Streamers.FirstOrDefault(item => item.streamerId == listOfStreamers[x].streamerId);

                        if (streamer.isLive == true) {
                            streamer.isLive = false;
                            context.SaveChanges();
                        }
                        
                        NotificationHub.Current.Clients.All.SendAsync($"{streamer.id}Live",
                            false);
                    }
                }
            }
        }


        public void RemoveTemp() {
            try {
                Directory.Delete(GlobalConfig.GetGlobalConfig("contentRootPath") + "tmp", true);
            } catch (DirectoryNotFoundException) {
                return;
            }
        }


        public void RefreshValidation() {
            Validation validation = new Validation();
            validation.Validate();
        }


        public void CheckForUpdates() {
            Notification notification = NotificationLogic.CreateNotification("softwareUpdate", Severity.Info, Position.General, "Checking for software updates...");
            Logger.Info("Checking for application updates...");
            new UpdateLogic().CheckForApplicationUpdates();
            Logger.Info("Checking for yt-dlp updates...");
            new UpdateYtDlp().CheckForYtDlpUpdates();
            NotificationLogic.DeleteNotification("softwareUpdate");
        }

        public void DatabaseBackup(string database) {
            Logger.Info($"Backing up {database} database...");
            Notification notification = NotificationLogic.CreateNotification("databaseBackup", Severity.Info, Position.General, "Backing up database...");
            int backupCount;
            using (var context = new MainDataContext()) {
                backupCount = context.Backups.Count(item => item.type == database);
            }

            if (backupCount > 5) {
                // trim database backups
                using (var context = new MainDataContext()) {
                    Backup oldestBackup = context.Backups.Where(item => item.type == database)
                        .OrderBy(item => item.datetime).First();
                    FileInfo backupFile = new FileInfo(oldestBackup.location);
                    if (backupFile.Exists) {
                        backupFile.Delete();
                    }

                    context.Remove(oldestBackup);
                    context.SaveChanges();
                }
            }

            DatabaseBackupLogic databaseBackupLogic = new DatabaseBackupLogic();
            if (database == "chatDb") {
                databaseBackupLogic.BackupChatDatabase();
            } else {
                databaseBackupLogic.BackupDatabase("mainDb");
            }

            Logger.Info($"Backed up {database} database.");
            NotificationLogic.DeleteNotification("databaseBackup");
        }

        // removes any in-progress live stream downloads since the last run from the database
        public Task RemoveLeftOverLiveStreamDownloads() {
            List<Stream> streams;
            Console.WriteLine("Checking for dead streams...");
            using (var mainDataContext = new MainDataContext()) {
                streams = mainDataContext.Streams.Where(stream => stream.vodId != 0 && stream.downloading).ToList();
                mainDataContext.RemoveRange(streams);

                if (streams.Count > 0) {
                    using (var chatDataContext = new ChatDataContext()) {
                        chatDataContext.RemoveRange(chatDataContext.Chats.Where(chat => streams.Select(stream => stream.streamId).Contains(chat.streamId)));
                        chatDataContext.SaveChanges();
                    }
                }

                mainDataContext.SaveChanges();
            }


            Console.WriteLine("Done!");
            return Task.CompletedTask;
        }
    }
}