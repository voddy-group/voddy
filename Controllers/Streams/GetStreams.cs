using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Streams {
    [ApiController]
    [Route("streams")]
    public class GetStreams : ControllerBase {
        [HttpGet]
        [Route("getStreams")]
        public HandleDownloadStreamsLogic.GetStreamsResult GetMultipleStreams(int id) {
            var streams = FetchStreams(id);

            return streams;
        }

        [HttpGet]
        [Route("getStreamsWithFilter")]
        public HandleDownloadStreamsLogic.GetStreamsResult GetStreamsWithFilter(int id) {
            var externalStreams = FetchStreams(id);

            using (var context = new DataContext()) {
                var internalStreams = context.Streams.ToList().Where(t => t.streamerId == id).ToList();
                //todo take another look at this
                var externalStreamsConverted = new List<Stream>();
                foreach (var stream in externalStreams.data) {
                    externalStreamsConverted.Add(new Stream {
                        streamId = int.Parse(stream.id),
                        streamerId = int.Parse(stream.user_id)
                    });
                }

                var alreadyExistingStreams = internalStreams.Except(externalStreamsConverted).ToList();

                foreach (var existingStream in alreadyExistingStreams) {
                    var stream =
                        externalStreams.data.FirstOrDefault(item => int.Parse(item.id) == existingStream.streamId);

                    if (stream != null) {
                        stream.alreadyAdded = true;
                        stream.downloading = existingStream.downloading;
                        stream.size = existingStream.size;
                        if (existingStream.thumbnailLocation != null)
                            stream.thumbnail_url = existingStream.thumbnailLocation;
                    }
                }

                /*for (var v = 0; v < newInternalStreams.Count; v++) {
                    externalStreams.data.Add(new HandleDownloadStreams.Data {
                        id = newInternalStreams[v].streamId.ToString(),
                        title = newInternalStreams[v].title,
                        thumbnail_url = newInternalStreams[v].thumbnailLocation,
                        view_count = 0,
                        duration = newInternalStreams[v].duration.ToString(),
                        created_at = newInternalStreams[v].createdAt,
                        alreadyAdded = true
                    });
                }*/

                return externalStreams;
            }
        }

        public HandleDownloadStreamsLogic.GetStreamsResult FetchStreams(int id) {
            bool isLive = false;
            using (var context = new DataContext()) {
                var data = context.Streamers.FirstOrDefault(item => Convert.ToInt32(item.streamerId) == id);

                if (data != null) isLive = data.isLive;
            }

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                          $"?user_id={id}" +
                                                          "&first=100", Method.GET);
            var deserializeResponse =
                JsonConvert.DeserializeObject<HandleDownloadStreamsLogic.GetStreamsResult>(response.Content);
            HandleDownloadStreamsLogic.GetStreamsResult getStreamsResult =
                new HandleDownloadStreamsLogic.GetStreamsResult();
            getStreamsResult.data = new List<HandleDownloadStreamsLogic.Data>();
            string cursor;
            foreach (var stream in deserializeResponse.data) {
                if (isLive && stream.thumbnail_url.Length > 0) {
                    getStreamsResult.data.Add(stream);
                } else if (!isLive) {
                    getStreamsResult.data.Add(stream);
                }
            }

            if (deserializeResponse.pagination.cursor != null && deserializeResponse.data.Count >= 100) {
                cursor = deserializeResponse.pagination.cursor;
            } else {
                cursor = null;
            }

            while (cursor != null) {
                var paginatedResponse = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                                       $"?user_id={id}" +
                                                                       "&first=100" +
                                                                       $"&after={deserializeResponse.pagination.cursor}",
                    Method.GET);
                deserializeResponse =
                    JsonConvert.DeserializeObject<HandleDownloadStreamsLogic.GetStreamsResult>(paginatedResponse
                        .Content);


                foreach (var stream in deserializeResponse.data) {
                    if (isLive && stream.thumbnail_url.Length > 0) {
                        getStreamsResult.data.Add(stream);
                    } else if (!isLive) {
                        getStreamsResult.data.Add(stream);
                    }
                }

                if (deserializeResponse.data.Count >= 100) {
                    cursor = deserializeResponse.pagination.cursor;
                } else {
                    cursor = null;
                }
            }

            for (int x = 0; x < getStreamsResult.data.Count; x++) {
                /*if (getStreamsResult.data[x].type != "archive") {
                    // only retrieve vods
                    getStreamsResult.data.Remove(getStreamsResult.data[x]);
                }*/

                // manually add thumbnail dimensions because twitch is too lazy to do it
                getStreamsResult.data[x].thumbnail_url = getStreamsResult.data[x].thumbnail_url
                    .Replace("%{width}", "320").Replace("%{height}", "180");
            }

            return getStreamsResult;
        }

        public class StreamCollection {
            private List<Stream> Data { get; set; }
        }
    }
}