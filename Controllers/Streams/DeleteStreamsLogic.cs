using System;
using System.IO;
using System.Linq;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using NLog;
using RestSharp;
using voddy.Databases.Chat;
using voddy.Databases.Main;
using voddy.Exceptions.Quartz;

namespace voddy.Controllers.Streams {
    public class DeleteStreamsLogic {
        private static Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();
        public DeleteStreamReturn DeleteSingleStreamLogic(long streamId) {
            using (var context = new MainDataContext()) {
                using (var chatContext = new ChatDataContext()) {
                    var stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                    if (stream == null) {
                        stream = context.Streams.FirstOrDefault(item => item.vodId == streamId); // add live stream delete capabilities
                    }
                    var chat = chatContext.Chats.Where(item => item.streamId == streamId).ToList();

                    if (stream != null) {
                        if (stream.downloadJobId != null) {
                            var splitJobKey = stream.downloadJobId.Split(".");
                            try {
                                JobHelpers.CancelJob(splitJobKey[1], splitJobKey[0], QuartzSchedulers.PrimaryScheduler());
                            } catch (MissingJobException e) {
                                _logger.Info(e.Message);
                            }
                        }

                        if (stream.vodId != 0) {
                            try {
                                CleanUpStreamFiles(GlobalConfig.GetGlobalConfig("contentRootPath"), stream.vodId, stream.streamerId);
                            } catch (DirectoryNotFoundException) {
                                CleanUpStreamFiles(GlobalConfig.GetGlobalConfig("contentRootPath"), stream.streamId, stream.streamerId);
                            }
                        } else {
                            CleanUpStreamFiles(GlobalConfig.GetGlobalConfig("contentRootPath"), stream.streamId, stream.streamerId);
                        }

                        context.Remove(stream);

                        if (stream.chatDownloadJobId != null) {
                            BackgroundJob.Delete(stream.chatDownloadJobId);
                            for (var x = 0; x < chat.Count; x++) {
                                chatContext.Remove(chat[x]);
                            }
                        }
                    }

                    context.SaveChanges();
                    chatContext.SaveChanges();
                }
            }

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var request =
                twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos?id=" + streamId, Method.GET);

            if (request.StatusCode == HttpStatusCode.OK) {
                return new DeleteStreamReturn {
                    isStillAvailable = true
                };
            }

            return new DeleteStreamReturn {
                isStillAvailable = false
            };
        }

        public void DeleteStreamerStreamsLogic(int streamerId) {
            using (var context = new MainDataContext()) {
                var streamerStreams = context.Streams.Where(item => item.streamerId == streamerId).ToList();

                for (var x = 0; x < streamerStreams.Count; x++) {
                    DeleteSingleStreamLogic(streamerStreams[x].streamId);
                }
            }
        }

        public void CleanUpStreamFiles(string contentRootPath, long streamId, int streamerId) {
            DirectoryInfo streamDir = new DirectoryInfo($"{contentRootPath}streamers/{streamerId}/vods/{streamId}");
            if (streamDir.Exists) {
                Directory.Delete(streamDir.FullName, true);
            }
        }
    }

    public class DeleteStreamReturn {
        public bool isStillAvailable { get; set; }
    }
}