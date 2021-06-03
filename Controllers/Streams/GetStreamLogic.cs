using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Streams {
    public class GetStreamLogic {
        public HandleDownloadStreamsLogic.GetStreamsResult GetStreamsWithFiltersLogic(int id, string cursor) {
            var externalStreams = FetchStreams(id, cursor);

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
                        stream.url = existingStream.downloadLocation;
                        stream.downloading = existingStream.downloading;
                        stream.size = existingStream.size;
                        if (existingStream.thumbnailLocation != null)
                            stream.thumbnail_url = existingStream.thumbnailLocation;
                    }
                }

                return externalStreams;
            }
        }

        public HandleDownloadStreamsLogic.GetStreamsResult FetchStreams(int id, string cursor = null) {
            int pageCount = 30;
            bool isLive = false;
            using (var context = new DataContext()) {
                var data = context.Streamers.FirstOrDefault(item => Convert.ToInt32(item.streamerId) == id);

                if (data != null) isLive = data.isLive;
            }

            IRestResponse response;
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            if (!String.IsNullOrEmpty(cursor) && cursor != "null" && cursor != "undefined") {
                response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                              $"?user_id={id}" +
                                                              $"&first={pageCount}" + 
                    $"&after={cursor}", Method.GET);
            } else {
                response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                              $"?user_id={id}" +
                                                              $"&first={pageCount}", Method.GET);
            }
            
            var getStreamsResult =
                JsonConvert.DeserializeObject<HandleDownloadStreamsLogic.GetStreamsResult>(response.Content);
            
            // get next set to see if it is the last page. Thanks twitch for always returning a page cursor, even when there is no more data BroBalt
            if (getStreamsResult.data.Count == pageCount) {
                IRestResponse nextPage;
                if (!String.IsNullOrEmpty(cursor) && cursor != "null" && cursor != "undefined") {
                    nextPage = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                                  $"?user_id={id}" +
                                                                  "&first=1", Method.GET);
                } else {
                    nextPage = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                              $"?user_id={id}" +
                                                              "&first=1", Method.GET);
                }

                var nextPageDeserialized =
                    JsonConvert.DeserializeObject<HandleDownloadStreamsLogic.GetStreamsResult>(nextPage.Content);

                if (nextPageDeserialized.data.Count == 0) {
                    // if no data in next page, remove the cursor
                    getStreamsResult.pagination.cursor = null;
                } 
            } else {
                getStreamsResult.pagination.cursor = null;
            }
            
            /*HandleDownloadStreamsLogic.GetStreamsResult getStreamsResult =
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
            }*/

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