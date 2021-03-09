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
using static voddy.DownloadHelpers;
using Stream = voddy.Models.Stream;

namespace voddy.Controllers {
    [ApiController]
    [Route("backgroundTask")]
    public class HandleDownloadStreams : ControllerBase {
        private readonly ILogger<HandleDownloadStreams> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IWebHostEnvironment _environment;
        private static string baseStreamUrl = "https://www.twitch.tv/videos/";
        private static string streamDirectory { get; set; }

        public HandleDownloadStreams(ILogger<HandleDownloadStreams> logger, IBackgroundJobClient backgroundJobClient,
            IWebHostEnvironment environment) {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _environment = environment;
        }

        [HttpPost]
        [Route("downloadStreams")]
        public IActionResult DownloadStreams([FromBody] GetStreamsResult streams, int id) {
            using (var context = new DataContext()) {
                foreach (var stream in streams.data) {
                    //PrepareDownload(stream, context);
                }
            }

            return Ok();
        }

        private bool PrepareDownload(Data stream, DataContext context) {
            var streamUrl = baseStreamUrl + stream.id;
            streamDirectory = $"{_environment.ContentRootPath}streamers/{stream.user_id}/vods/{stream.id}";
            var dbStream = context.Streams.FirstOrDefault(item => item.streamId == Int32.Parse(stream.id));
            if (dbStream != null) {
                return false;
            }

            YoutubeDlVideoJson.YoutubeDlVideoInfo youtubeDlVideoInfo = GetDownloadQualityUrl(streamUrl);

            Directory.CreateDirectory(streamDirectory);

            DownloadFile(stream.thumbnail_url, $"{streamDirectory}/thumbnail.jpg");

            string outputPath = new string(Path.Combine(
                    $"{streamDirectory}/{youtubeDlVideoInfo.filename}").ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());

            //TODO more should be queued, not done immediately
            _backgroundJobClient.Enqueue(() =>
                DownloadStream(Int32.Parse(stream.id), outputPath, youtubeDlVideoInfo.url));

            dbStream = new Stream {
                streamId = Int32.Parse(stream.id),
                streamerId = Int32.Parse(stream.user_id),
                quality = youtubeDlVideoInfo.quality,
                title = stream.title,
                createdAt = stream.created_at,
                downloadLocation = outputPath,
                thumbnailLocation = $"{streamDirectory}/thumbnail.jpg",
                duration = TimeSpan.FromSeconds(youtubeDlVideoInfo.duration),
                downloading = true
            };

            context.Add(dbStream);
            context.SaveChanges();

            return true;
        }

        [HttpPost]
        [Route("downloadStream")]
        public IActionResult DownloadSingleStream([FromBody] Data stream) {
            using (var context = new DataContext()) {
                if (PrepareDownload(stream, context)) {
                    return Ok();
                }
            }
            return Conflict("Already exists.");
        }

        [HttpGet]
        [Route("getStreams")]
        public GetStreamsResult GetStreams(int id) {
            var streams = FetchStreams(id);

            return streams;
        }

        [HttpGet]
        [Route("getStreamsWithFilter")]
        public GetStreamsResult GetStreamsWithFilter(int id) {
            var streams = FetchStreams(id);

            using (var context = new DataContext()) {
                for (int x = 0; x < streams.data.Count; x++) {
                    var dbStream = context.Streams.FirstOrDefault(item => item.streamId == Int32.Parse(streams.data[x].id));

                    if (dbStream != null) {
                        streams.data[x].alreadyAdded = true;
                    } else {
                        streams.data[x].alreadyAdded = false;
                    }
                }
            }

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

        public void DownloadStream(int streamId, string outputPath, string url) {
            string youtubeDlPath = GetYoutubeDlPath();
            
            Console.WriteLine($"{url} -o {outputPath}");

            var processInfo = new ProcessStartInfo(youtubeDlPath, $"{url} -o {outputPath}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            SetDownloadToFinished(streamId);
        }

        private YoutubeDlVideoJson.YoutubeDlVideoInfo GetDownloadQualityUrl(string streamUrl) {
            var processInfo = new ProcessStartInfo(GetYoutubeDlPath(), $"--dump-json {streamUrl}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            string json = process.StandardOutput.ReadLine();
            process.WaitForExit();


            var deserializedJson = JsonConvert.DeserializeObject<YoutubeDlVideoJson.YoutubeDlVideo>(json);
            var returnValue = new YoutubeDlVideoJson.YoutubeDlVideoInfo();

            string quality = "source"; //TODO make this dynamic

            if (quality == "source") {
                returnValue.url = deserializedJson.url;
                returnValue.quality = deserializedJson.height;
            }

            returnValue.duration = deserializedJson.duration;
            returnValue.filename = deserializedJson._filename;

            return returnValue;
        }

        private void SetDownloadToFinished(int streamId) {
            using (var context = new DataContext()) {
                var dbStream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                dbStream.downloading = false;
                context.SaveChanges();
            }
        }

        private string GetYoutubeDlPath() {
            Config youtubeDlInstance = new Config();
            using (var context = new DataContext()) {
                youtubeDlInstance =
                    context.Configs.FirstOrDefault(item => item.key == "youtube-dl");
            }

            if (youtubeDlInstance.value != null) {
                return youtubeDlInstance.value;
            }

            return "youtube-dl";
        }

        public class Data {
            public bool alreadyAdded { get; set; }
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