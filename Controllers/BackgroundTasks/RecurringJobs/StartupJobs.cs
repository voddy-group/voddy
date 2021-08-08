using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Database;
using voddy.Controllers.Structures;
using voddy.Controllers.Setup.Update;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;
using Stream = voddy.Databases.Main.Models.Stream;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs {
    public class StartupJobs {
        [Queue("default")]
        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void RequeueOrphanedJobs() {
            Console.WriteLine("Checking for orphaned jobs...");
            string uuid = NotificationLogic.SendNotification("info", "Checking for orphaned jobs...");
            var api = JobStorage.Current.GetMonitoringApi();
            var processingJobs = api.ProcessingJobs(0, 100);
            var servers = api.Servers();
            var orphanJobs = processingJobs.Where(j => servers.All(s => s.Name != j.Value.ServerId));
            foreach (var orphanJob in orphanJobs) {
                Console.WriteLine($"Queueing {orphanJob.Key}.");
                BackgroundJob.Requeue(orphanJob.Key);
            }

            Console.WriteLine("Done!");
            NotificationLogic.DeleteNotification(uuid);
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void StreamerCheckForUpdates() {
            string uuid = NotificationLogic.SendNotification("info", "Checking for streamer updates...");
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
            NotificationLogic.DeleteNotification(uuid);
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

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void TrimLogs() {
            using (var context = new MainDataContext()) {
                var records = context.Logs.AsEnumerable().OrderByDescending(item => DateTime.Parse(item.logged))
                    .Skip(7500);
                foreach (var log in records) {
                    context.Remove(log);
                }

                context.SaveChanges();
            }
        }

        [Queue("default")]
        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void CheckForStreamerLiveStatus() {
            string uuid = NotificationLogic.SendNotification("info", "Checking for live streamers...");
            Console.WriteLine("Checking for live streams to download...");
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

            Console.WriteLine("Done!");
            NotificationLogic.DeleteNotification(uuid);
        }


        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
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
            HandleDownloadStreamsLogic.GetStreamsResult liveStream =
                JsonConvert.DeserializeObject<HandleDownloadStreamsLogic.GetStreamsResult>(response.Content);

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

                        if (streamer.getLive == false || alreadyExistingStream != null) {
                            // already downloading/downloaded, or user does not want to download this streamers live stream
                            continue;
                        }
                    }

                    if (DateTime.UtcNow.Subtract(stream.started_at).TotalMinutes < 5) {
                        // if stream started less than 5 minutes ago
                        using (var context = new MainDataContext()) {
                            var dbStreamer = context.Streamers.FirstOrDefault(item =>
                                item.streamerId == listOfStreamers[x].streamerId);

                            if (dbStreamer != null) {
                                dbStreamer.isLive = true;

                                HandleDownloadStreamsLogic handleDownloadStreamsLogic =
                                    new HandleDownloadStreamsLogic();
                                StreamExtended convertedLiveStream = new StreamExtended {
                                    streamerId = dbStreamer.streamerId,
                                    streamId = Int64.Parse(stream.id),
                                    thumbnailLocation = stream.thumbnail_url.Replace("{width}", "320")
                                        .Replace("{height}", "180"),
                                    title = stream.title,
                                    createdAt = stream.started_at
                                };
                                BackgroundJob.Enqueue(() =>
                                    handleDownloadStreamsLogic.PrepareDownload(convertedLiveStream, true));
                            }

                            context.SaveChanges();
                        }
                    }
                } else {
                    using (var context = new MainDataContext()) {
                        var streamer =
                            context.Streamers.FirstOrDefault(item => item.streamerId == listOfStreamers[x].streamerId);

                        if (streamer.isLive == true) {
                            streamer.isLive = false;
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void RemoveTemp() {
            string contentRootPath;
            using (var context = new MainDataContext()) {
                contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value;
            }

            try {
                Directory.Delete(contentRootPath + "tmp", true);
            } catch (DirectoryNotFoundException) {
                return;
            }
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void RefreshValidation() {
            Validation validation = new Validation();
            validation.Validate();
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void CheckForUpdates() {
            string uuid = NotificationLogic.SendNotification("info", "Checking for application updates...");
            Console.WriteLine("Checking for application updates...");
            UpdateLogic update = new UpdateLogic();
            update.CheckForUpdates();
            NotificationLogic.DeleteNotification(uuid);
        }

        public void DatabaseBackup(string database) {
            Console.WriteLine($"Backing up {database} database...");
            string uuid = NotificationLogic.SendNotification("info", "Backing up database...");
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

            Console.WriteLine($"Backed up {database} database.");
            NotificationLogic.DeleteNotification(uuid);
        }
    }
}