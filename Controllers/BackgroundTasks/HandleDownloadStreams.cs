using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Structures;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
    [ApiController]
    [Route("backgroundTask")]
    public class HandleDownloadStreams : ControllerBase {
        private readonly ILogger<HandleDownloadStreams> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IWebHostEnvironment _environment;
        private string streamUrl = "https://www.twitch.tv/videos/";

        public HandleDownloadStreams(ILogger<HandleDownloadStreams> logger, IBackgroundJobClient backgroundJobClient,
            IWebHostEnvironment environment) {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _environment = environment;
        }

        [HttpPost]
        [Route("downloadStreams")]
        public void DownloadStreams([FromBody] GetStreamsResult streams, int id) {
            //GetStreamsResult streams = FetchStreams(id);

            foreach (var stream in streams.data) {
                streamUrl += stream.id;
                
                YoutubeDlVideoJson.YoutubeDlVideo youtubeDlVideo = GetDownloadQuality();

                _backgroundJobClient.Enqueue(() =>
                    DownloadStream(Int32.Parse(stream.user_id), Int32.Parse(stream.id), stream.title, youtubeDlVideo));
            }
        }

        [HttpPost]
        [Route("downloadStream")]
        public IActionResult DownloadSingleStream([FromBody] Data stream) {
            streamUrl += stream.id;
            using (var context = new DataContext()) {
                var dbStream = context.Streams.FirstOrDefault(item => item.streamId == Int32.Parse(stream.id));
                if (dbStream != null) {
                    return Conflict("Already exists.");
                }

                YoutubeDlVideoJson.YoutubeDlVideo youtubeDlVideo = GetDownloadQuality();


                _backgroundJobClient.Enqueue(() =>
                    DownloadStream(Int32.Parse(stream.user_id), Int32.Parse(stream.id), stream.title, youtubeDlVideo));
                dbStream.streamId = Int32.Parse(stream.id);
                dbStream.streamerId = Int32.Parse(stream.user_id);
            }

            return Ok();
        }

        [HttpGet]
        [Route("getStreams")]
        public GetStreamsResult GetStreams(int id) {
            var streams = FetchStreams(id);

            return streams;
        }

        public GetStreamsResult FetchStreams(int id) {
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
                                                                       $"&after={deserializeResponse.pagination.cursor}",
                    Method.GET);
                deserializeResponse = JsonConvert.DeserializeObject<GetStreamsResult>(paginatedResponse.Content);
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

        public void DownloadStream(int userId, int streamId, string title,
            YoutubeDlVideoJson.YoutubeDlVideo youtubeDlVideo) {
            string youtubeDlPath = GetYoutubeDlPath();

            Directory.CreateDirectory($"{_environment.ContentRootPath}streamers/{userId}/vods/{streamId}");

            string outputPath =
                Path.Combine(
                    $"{_environment.ContentRootPath}streamers/{userId}/vods/{streamId}/{streamId}-{RemoveSpecialCharacters(title)}");

            string quality = "source"; //TODO make this dynamic

            if (quality == "source") {
                streamUrl = youtubeDlVideo.url;
            }

            Console.WriteLine(outputPath);

            var processInfo = new ProcessStartInfo(youtubeDlPath, $"{streamUrl} -o {outputPath}%(ext)s");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            process.WaitForExit();
        }

        private YoutubeDlVideoJson.YoutubeDlVideo GetDownloadQuality() {
            var processInfo = new ProcessStartInfo(GetYoutubeDlPath(), $"--dump-json {streamUrl}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            string json = process.StandardOutput.ReadLine();
            process.WaitForExit();

            return JsonConvert.DeserializeObject<YoutubeDlVideoJson.YoutubeDlVideo>(json);
        }

        private string GetYoutubeDlPath() {
            Executable youtubeDlInstance = new Executable();
            using (var context = new DataContext()) {
                youtubeDlInstance =
                    context.Executables.FirstOrDefault(item => item.name == "youtube-dl");
            }

            if (youtubeDlInstance.path != null) {
                return youtubeDlInstance.path;
            }

            return "youtube-dl";
        }

        public static string RemoveSpecialCharacters(string str) {
            char[] buffer = new char[str.Length];
            int idx = 0;

            foreach (char c in str) {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z')
                                           || (c >= 'a' && c <= 'z') || (c == '.') || (c == '_') || (c == ',')) {
                    buffer[idx] = c;
                    idx++;
                }
            }

            return new string(buffer, 0, idx);
        }

        public class Data {
            public string id { get; set; }
            public string user_id { get; set; }
            public string user_login { get; set; }
            public string user_name { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public DateTime created_at { get; set; }
            public DateTime published_at { get; set; }
            public string url { get; set; }
            public string thumbnail_url { get; set; }
            public string viewable { get; set; }
            public int view_count { get; set; }
            public string language { get; set; }
            public string type { get; set; }
            public string duration { get; set; }
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