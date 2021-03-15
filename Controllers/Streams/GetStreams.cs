using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using voddy.Data;

namespace voddy.Controllers.Streams {
    [ApiController]
    [Route("streams")]
    public class GetStreams : ControllerBase {
        [HttpGet]
        [Route("getStreams")]
        public HandleDownloadStreams.GetStreamsResult GetMultipleStreams(int id) {
            var streams = FetchStreams(id);

            return streams;
        }

        [HttpGet]
        [Route("getStreamsWithFilter")]
        public HandleDownloadStreams.GetStreamsResult GetStreamsWithFilter(int id) {
            var streams = FetchStreams(id);

            using (var context = new DataContext()) {
                for (int x = 0; x < streams.data.Count; x++) {
                    var dbStream =
                        context.Streams.FirstOrDefault(item => item.streamId == Int32.Parse(streams.data[x].id));

                    if (dbStream != null) {
                        streams.data[x].alreadyAdded = true;
                        streams.data[x].downloading = dbStream.downloading;
                    } else {
                        streams.data[x].alreadyAdded = false;
                    }
                }
            }

            return streams;
        }

        public HandleDownloadStreams.GetStreamsResult FetchStreams(int id) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                          $"?user_id={id}" +
                                                          "&first=100", Method.GET);
            var deserializeResponse = JsonConvert.DeserializeObject<HandleDownloadStreams.GetStreamsResult>(response.Content);
            HandleDownloadStreams.GetStreamsResult getStreamsResult = new HandleDownloadStreams.GetStreamsResult();
            getStreamsResult.data = new List<HandleDownloadStreams.Data>();
            var cursor = "";
            foreach (var stream in deserializeResponse.data) {
                getStreamsResult.data.Add(stream);
            }

            if (deserializeResponse.pagination.cursor != null) {
                cursor = deserializeResponse.pagination.cursor;
            }

            while (cursor != null) {
                var paginatedResponse = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                                       $"?user_id={id}" +
                                                                       "&first=100" +
                                                                       $"&after={deserializeResponse.pagination.cursor}",
                    Method.GET);
                deserializeResponse = JsonConvert.DeserializeObject<HandleDownloadStreams.GetStreamsResult>(paginatedResponse.Content);
                foreach (var stream in deserializeResponse.data) {
                    getStreamsResult.data.Add(stream);
                }

                cursor = deserializeResponse.pagination.cursor;
            }

            for (int x = 0; x < getStreamsResult.data.Count; x++) {
                if (getStreamsResult.data[x].type != "archive") {
                    // only retrieve vods
                    getStreamsResult.data.Remove(getStreamsResult.data[x]);
                }

                // manually add thumbnail dimensions because twitch is too lazy to do it
                getStreamsResult.data[x].thumbnail_url = getStreamsResult.data[x].thumbnail_url
                    .Replace("%{width}", "320").Replace("%{height}", "180");
            }

            return getStreamsResult;
        }
    }
}