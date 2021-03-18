using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
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
                    if (!stream.alreadyAdded) {
                        PrepareDownload(stream, context);
                    }
                }
            }

            return Ok();
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

        

        private bool PrepareDownload(Data stream, DataContext context) {
            var streamUrl = baseStreamUrl + stream.id;
            streamDirectory = $"{_environment.ContentRootPath}streamers/{stream.user_id}/vods/{stream.id}";
            var dbStream = context.Streams.FirstOrDefault(item => item.streamId == Int32.Parse(stream.id));


            YoutubeDlVideoJson.YoutubeDlVideoInfo youtubeDlVideoInfo = GetDownloadQualityUrl(streamUrl);

            Directory.CreateDirectory(streamDirectory);

            if (!string.IsNullOrEmpty(stream.thumbnail_url)) { //todo handle missing thumbnail, maybe use youtubedl generated thumbnail instead
                DownloadFile(stream.thumbnail_url, $"{streamDirectory}/thumbnail.jpg");
            }

            string outputPath = new string(Path.Combine(
                    $"{streamDirectory}/{youtubeDlVideoInfo.filename}").ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());

            //TODO more should be queued, not done immediately
            string jobId = _backgroundJobClient.Enqueue(() =>
                DownloadStream(Int32.Parse(stream.id), outputPath, youtubeDlVideoInfo.url, CancellationToken.None));

            if (dbStream != null) {
                dbStream.streamId = Int32.Parse(stream.id);
                dbStream.streamerId = Int32.Parse(stream.user_id);
                dbStream.quality = youtubeDlVideoInfo.quality;
                dbStream.url = youtubeDlVideoInfo.url;
                dbStream.title = stream.title;
                dbStream.createdAt = stream.created_at;
                dbStream.downloadLocation = outputPath;
                dbStream.thumbnailLocation = $"{streamDirectory}/thumbnail.jpg";
                dbStream.duration = TimeSpan.FromSeconds(youtubeDlVideoInfo.duration);
                dbStream.downloading = true;
                dbStream.downloadJobId = jobId;
            } else {
                PrepareChat(Int32.Parse(stream.id));
                // only download chat if this is a new vod

                dbStream = new Stream {
                    streamId = Int32.Parse(stream.id),
                    streamerId = Int32.Parse(stream.user_id),
                    quality = youtubeDlVideoInfo.quality,
                    title = stream.title,
                    url = youtubeDlVideoInfo.url,
                    createdAt = stream.created_at,
                    downloadLocation = outputPath,
                    thumbnailLocation = $"{streamDirectory}/thumbnail.jpg",
                    duration = TimeSpan.FromSeconds(youtubeDlVideoInfo.duration),
                    downloading = true,
                    downloadJobId = jobId
                };

                context.Add(dbStream);
            }

            context.SaveChanges();

            return true;
        }
        

        public static async Task DownloadStream(int streamId, string outputPath, string url, CancellationToken token) {
            string youtubeDlPath = GetYoutubeDlPath();

            Console.WriteLine($"{url} -o {outputPath}");

            var processInfo = new ProcessStartInfo(youtubeDlPath, $"{url} -o {outputPath}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                Console.WriteLine("output>>" + e.Data);
                if (token.IsCancellationRequested) {
                    process.Kill(); // insta kill
                    process.WaitForExit();
                }
            };
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                Console.WriteLine("error>>" + e.Data);
                if (token.IsCancellationRequested) {
                    process.Kill(); // insta kill
                    process.WaitForExit();
                }
            };
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            SetDownloadToFinished(streamId);
        }

        private static YoutubeDlVideoJson.YoutubeDlVideoInfo GetDownloadQualityUrl(string streamUrl) {
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

        private static void SetDownloadToFinished(int streamId) {
            using (var context = new DataContext()) {
                var dbStream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                dbStream.downloading = false;
                context.SaveChanges();
            }
        }

        private static string GetYoutubeDlPath() {
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


        public void PrepareChat(int streamId) {
            string jobId = _backgroundJobClient.Enqueue(() => DownloadChat(streamId, CancellationToken.None));
            using (var context = new DataContext()) {
                var message = context.Chats.FirstOrDefault(item => item.streamId == streamId);

                if (message == null) {
                    Chat chat = new Chat();
                    chat.streamId = streamId;
                    chat.downloading = true;
                    chat.downloadJobId = jobId;
                    context.Add(chat);
                }

                context.SaveChanges();
            }
        }

        public static async Task DownloadChat(int streamId, CancellationToken token) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response =
                twitchApiHelpers.LegacyTwitchRequest($"https://api.twitch.tv/v5/videos/{streamId}/comments",
                    Method.GET);
            var deserializeResponse = JsonConvert.DeserializeObject<ChatMessageJsonClass.ChatMessage>(response.Content);
            ChatMessageJsonClass.ChatMessage chatMessage = new ChatMessageJsonClass.ChatMessage();
            chatMessage.comments = new List<ChatMessageJsonClass.Comment>();
            var cursor = "";
            foreach (var comment in deserializeResponse.comments) {
                chatMessage.comments.Add(comment);
            }

            if (deserializeResponse._next != null) {
                cursor = deserializeResponse._next;
            }

            while (cursor != null) {
                Console.WriteLine("Getting more chat..");
                if (token.IsCancellationRequested) {
                    return; // insta kill 
                }

                var paginatedResponse = twitchApiHelpers.LegacyTwitchRequest(
                    $"https://api.twitch.tv/v5/videos/{streamId}/comments" +
                    $"?cursor={deserializeResponse._next}", Method.GET);
                deserializeResponse =
                    JsonConvert.DeserializeObject<ChatMessageJsonClass.ChatMessage>(paginatedResponse.Content);
                foreach (var comment in deserializeResponse.comments) {
                    chatMessage.comments.Add(comment);
                }

                cursor = deserializeResponse._next;
            }

            using (var context = new DataContext()) {
                var jsonMessage = JsonConvert.SerializeObject(chatMessage);
                var message = context.Chats.FirstOrDefault(item => item.streamId == streamId);

                if (message != null) {
                    message.comment = jsonMessage;
                    message.downloading = false;
                } else {
                    Chat chat = new Chat {
                        streamId = streamId,
                        comment = jsonMessage,
                        downloading = false
                    };
                    context.Add(chat);
                }

                await context.SaveChangesAsync(token);
            }
        }

        public class Data {
            public bool downloading { get; set; }
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