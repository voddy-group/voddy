using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.BackgroundTasks;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Streams {
    public class GetStreamLogic {
        public List<StreamExtended> GetStreamsWithFiltersLogic(int id) {
            var externalStreams = FetchStreams(id);

            List<StreamExtended> toReturn = new List<StreamExtended>();

            using (var context = new MainDataContext()) {
                var internalStreams = context.Streams.ToList().Where(t => t.streamerId == id).ToList();
                List<StreamExtended> convertedStreams = new List<StreamExtended>();
                foreach (var stream in internalStreams) {
                    convertedStreams.Add(
                        JsonConvert.DeserializeObject<StreamExtended>(JsonConvert.SerializeObject(stream)));
                }

                toReturn = convertedStreams;

                //todo take another look at this
                var externalStreamsConverted = new List<Stream>();
                foreach (var stream in externalStreams.data) {
                    var existingStream = toReturn.FirstOrDefault(item => item.streamId == Int64.Parse(stream.id));

                    if (existingStream == null) {
                        toReturn.Add(new StreamExtended {
                            id = -1,
                            streamId = Int64.Parse(stream.id),
                            streamerId = stream.user_id,
                            title = stream.title,
                            createdAt = stream.created_at,
                            thumbnailLocation = stream.thumbnail_url,
                            url = stream.url,
                            duration = (int)TimeSpan.ParseExact(
                                stream.duration.Replace("h", ":").Replace("m", ":").Replace("s", ""),
                                new string[] { @"h\:m\:s", @"m\:s", @"%s" }, CultureInfo.InvariantCulture).TotalSeconds
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

        public StreamHelpers.GetStreamsResult FetchStreams(int id) {
            bool isLive = false;
            using (var context = new MainDataContext()) {
                Streamer data = context.Streamers.FirstOrDefault(item => item.streamerId == id);

                if (data != null && data.isLive != null) isLive = (bool)data.isLive;
            }

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                          $"?user_id={id}" +
                                                          "&first=100", Method.GET);
            var deserializeResponse =
                JsonConvert.DeserializeObject<StreamHelpers.GetStreamsResult>(response.Content);
            StreamHelpers.GetStreamsResult getStreamsResult =
                new StreamHelpers.GetStreamsResult();
            getStreamsResult.data = new List<StreamHelpers.Data>();
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
                    JsonConvert.DeserializeObject<StreamHelpers.GetStreamsResult>(paginatedResponse
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