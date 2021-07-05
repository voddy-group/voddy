using System.IO;
using System.Linq;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using voddy.Data;

namespace voddy.Controllers.Streams {
    public class DeleteStreamsLogic {
        public DeleteStreamReturn DeleteSingleStreamLogic(long streamId) {
            using (var context = new DataContext()) {
                var stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                var chat = context.Chats.Where(item => item.streamId == streamId).ToList();

                if (stream != null) {
                    if (stream.downloadJobId != null) {
                        BackgroundJob.Delete(stream.downloadJobId);
                    }

                    string contentRootPath =
                        context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value;

                    if (stream.vodId != 0) {
                        try {
                            CleanUpStreamFiles(contentRootPath, stream.vodId, stream.streamerId);
                        } catch (DirectoryNotFoundException) {
                            CleanUpStreamFiles(contentRootPath, stream.streamId, stream.streamerId);
                        }
                    } else {
                        CleanUpStreamFiles(contentRootPath, stream.streamId, stream.streamerId);
                    }

                    context.Remove(stream);

                    if (stream.chatDownloadJobId != null) {
                        BackgroundJob.Delete(stream.chatDownloadJobId);
                        for (var x = 0; x < chat.Count; x++) {
                            context.Remove(chat[x]);
                        }
                    }
                }

                context.SaveChanges();
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
            using (var context = new DataContext()) {
                var streamerStreams = context.Streams.Where(item => item.streamerId == streamerId).ToList();

                for (var x = 0; x < streamerStreams.Count; x++) {
                    DeleteSingleStreamLogic(streamerStreams[x].streamId);
                }
            }
        }

        public void CleanUpStreamFiles(string contentRootPath, long streamId, int streamerId) {
            Directory.Delete($"{contentRootPath}streamers/{streamerId}/vods/{streamId}", true);
        }
    }

    public class DeleteStreamReturn {
        public bool isStillAvailable { get; set; }
    }
}