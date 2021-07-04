using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Streams {
    public class GetStreamLogic {
        public List<Stream> GetStreamsWithFiltersLogic(int id) {
            var externalStreams = FetchStreams(id);

            List<Stream> toReturn = new List<Stream>();

            using (var context = new DataContext()) {
                var internalStreams = context.Streams.ToList().Where(t => t.streamerId == id).ToList();

                toReturn = internalStreams;
                
                //todo take another look at this
                var externalStreamsConverted = new List<Stream>();
                foreach (var stream in externalStreams.data) {
                    var existingStream = toReturn.FirstOrDefault(item => item.streamId == Int64.Parse(stream.id));
                    
                    if (existingStream == null) {
                        toReturn.Add(new Stream {
                            id = -1,
                            streamId = Int64.Parse(stream.id),
                            streamerId = Int32.Parse(stream.user_id),
                            title = stream.title,
                            createdAt = stream.created_at,
                            thumbnailLocation = stream.thumbnail_url,
                            url = stream.url,
                            duration = TimeSpan.Parse(stream.duration.Replace("h", ":").Replace("m", ":").Replace("s", ""))
                        });
                    }
                    /*externalStreamsConverted.Add(new Stream {
                        streamId = int.Parse(stream.id),
                        streamerId = int.Parse(stream.user_id)
                    });*/
                }
                
                //var alreadyExistingStreams = internalStreams.Except(externalStreamsConverted).ToList();

                /*foreach (var existingStream in internalStreams) {
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
                }*/

                return toReturn.OrderByDescending(item => item.createdAt).ToList();
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
    }
}