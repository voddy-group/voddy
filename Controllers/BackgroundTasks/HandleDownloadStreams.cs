using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NYoutubeDL;
using RestSharp;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
    [ApiController]
    [Route("backgroundTask")]
    public class HandleDownloadStreams : ControllerBase {
        private readonly ILogger<HandleDownloadStreams> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IWebHostEnvironment _environment;

        public HandleDownloadStreams(ILogger<HandleDownloadStreams> logger, IBackgroundJobClient backgroundJobClient,
            IWebHostEnvironment environment) {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _environment = environment;
        }

        [HttpGet]
        [Route("downloadStreams")]
        public void DownloadStreams(int id) {
            GetStreamsResult streams = GetStreams(id);

            foreach (var stream in streams.data) {
                _backgroundJobClient.Enqueue(() => DownloadStream(Int32.Parse(stream.user_id), Int32.Parse(stream.id), stream.title));
            }
        }

        public GetStreamsResult GetStreams(int id) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                          $"?user_id={id}" +
                                                          "&first=100", Method.GET);
            var deserializeResponse = JsonConvert.DeserializeObject<GetStreamsResult>(response.Content);
            GetStreamsResult getStreamsResult = new GetStreamsResult();
            getStreamsResult.data = new List<Data>();
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
                                                                       $"&after={deserializeResponse.pagination.cursor}", Method.GET);
                deserializeResponse = JsonConvert.DeserializeObject<GetStreamsResult>(paginatedResponse.Content);
                foreach (var stream in deserializeResponse.data) {
                    getStreamsResult.data.Add(stream);
                }
                cursor = deserializeResponse.pagination.cursor;
            }

            return getStreamsResult;
        }
        
        public void DownloadStream(int userId, int streamId, string title) {
            YoutubeDL youtubeDl = new YoutubeDL();
            Executable youtubeDlInstance = new Executable();
            using (var context = new DataContext()) {
                youtubeDlInstance =
                    context.Executables.FirstOrDefault(item => item.name == "youtube-dl");
            }

            if (youtubeDlInstance.path != null) {
                youtubeDl.YoutubeDlPath = youtubeDlInstance.path;
            }

            youtubeDl.VideoUrl = $"https://www.twitch.tv/videos/{streamId}";

            Directory.CreateDirectory($"{_environment.ContentRootPath}/streamers/{userId}/{streamId}");

            youtubeDl.Options.FilesystemOptions.Output =
                Path.Combine($"{_environment.ContentRootPath}/streamers/{userId}/{streamId}/{streamId}-{title}");
            
            youtubeDl.Download();
        }

        public class Data {
            public string id { get; set; }
            public string user_id { get; set; }
            public string user_login { get; set; }
            public string user_name { get; set; }
            public string game_id { get; set; }
            public string game_name { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public int viewer_count { get; set; }
            public DateTime started_at { get; set; }
            public string language { get; set; }
            public string thumbnail_url { get; set; }
        }

        public class Pagination {
            public string cursor { get; set; }
        }

        public class GetStreamsResult {
            public List<Data> data { get; set; }
            public Pagination pagination { get; set; }
        }
    }
}