using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.LiveStreams;
using voddy.Controllers.Structures;
using voddy.Data;
using voddy.Models;
using Stream = voddy.Models.Stream;
using static voddy.DownloadHelpers;


namespace voddy.Controllers {
    public class HandleDownloadStreamsLogic {
        public bool PrepareDownload(Data stream, bool isLive) {
            string streamUrl;
            if (isLive) {
                streamUrl = "https://www.twitch.tv/" + stream.user_login;
            } else {
                streamUrl = "https://www.twitch.tv/videos/" + stream.id;
            }

            string streamDirectory = "";
            using (var context = new DataContext()) {
                var data = context.Configs.FirstOrDefault(item => item.key == "contentRootPath");

                if (data != null) {
                    var contentRootPath = data.value;
                    streamDirectory = $"{contentRootPath}streamers/{stream.user_id}/vods/{stream.id}";
                }
            }

            YoutubeDlVideoJson.YoutubeDlVideoInfo youtubeDlVideoInfo = GetDownloadQualityUrl(streamUrl, stream.user_id);

            Directory.CreateDirectory(streamDirectory);

            string thumbnailSaveLocation = $"/voddy/streamers/{stream.user_id}/vods/{stream.id}/thumbnail.jpg";

            if (!string.IsNullOrEmpty(stream.thumbnail_url)) {
                //todo handle missing thumbnail, maybe use youtubedl generated thumbnail instead
                DownloadFile(stream.thumbnail_url, $"{streamDirectory}/thumbnail.jpg");
            }

            string outputPath = new string(Path.Combine(
                    $"{streamDirectory}/{youtubeDlVideoInfo.filename}").ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
            
            //TODO more should be queued, not done immediately
            string jobId = BackgroundJob.Enqueue(() =>
                DownloadStream(Int64.Parse(stream.id), outputPath, youtubeDlVideoInfo.url, CancellationToken.None, isLive));

            Stream? dbStream;

            using (var context = new DataContext()) {
                dbStream = context.Streams.FirstOrDefault(item => item.streamId == Int64.Parse(stream.id));

                if (dbStream != null) {
                    dbStream.streamId = Int64.Parse(stream.id);
                    dbStream.streamerId = Int32.Parse(stream.user_id);
                    dbStream.quality = youtubeDlVideoInfo.quality;
                    dbStream.url = youtubeDlVideoInfo.url;
                    dbStream.title = stream.title;
                    dbStream.createdAt = stream.created_at;
                    dbStream.downloadLocation = outputPath;
                    dbStream.thumbnailLocation = thumbnailSaveLocation;
                    dbStream.duration = TimeSpan.FromSeconds(youtubeDlVideoInfo.duration);
                    dbStream.downloading = true;
                    dbStream.downloadJobId = jobId;
                } else {
                    string chatJobId;
                    if (isLive) {
                        chatJobId = PrepareLiveChat(stream.user_login, Int64.Parse(stream.id));

                        dbStream = new Stream {
                            streamId = Int64.Parse(stream.id),
                            streamerId = Int32.Parse(stream.user_id),
                            quality = youtubeDlVideoInfo.quality,
                            title = stream.title,
                            url = youtubeDlVideoInfo.url,
                            createdAt = stream.created_at,
                            downloadLocation = outputPath,
                            thumbnailLocation = thumbnailSaveLocation,
                            downloading = true,
                            downloadJobId = jobId,
                            chatDownloading = true,
                            chatDownloadJobId = chatJobId
                        };
                    } else {
                        chatJobId = PrepareChat(Int64.Parse(stream.id));

                        dbStream = new Stream {
                            streamId = Int64.Parse(stream.id),
                            streamerId = Int32.Parse(stream.user_id),
                            quality = youtubeDlVideoInfo.quality,
                            title = stream.title,
                            url = youtubeDlVideoInfo.url,
                            createdAt = stream.created_at,
                            downloadLocation = outputPath,
                            thumbnailLocation = thumbnailSaveLocation,
                            duration = TimeSpan.FromSeconds(youtubeDlVideoInfo.duration),
                            downloading = true,
                            downloadJobId = jobId,
                            chatDownloading = true,
                            chatDownloadJobId = chatJobId
                        };
                    }
                    // only download chat if this is a new vod


                    context.Add(dbStream);
                }

                context.SaveChanges();
            }

            //_hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());

            return true;
        }

        public string CheckForDownloadingStreams(bool skip = false) {
            int currentlyDownloading;
            using (var context = new DataContext()) {
                currentlyDownloading =
                    context.Streams.Count(item => item.downloading || item.chatDownloading);
            }

            if (currentlyDownloading > 0) {
                return $"Downloading {currentlyDownloading} streams/chats...";
            }

            return "";
        }

        [Queue("default")]
        public async Task DownloadStream(long streamId, string outputPath, string url, CancellationToken token, bool isLive) {
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
                    //_hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
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

            Console.WriteLine(isLive
                ? "Stream has gone offline, stopped downloading."
                : "VOD downloaded, stopped downloading");

            SetDownloadToFinished(streamId, isLive);
            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
        }

        private static YoutubeDlVideoJson.YoutubeDlVideoInfo
            GetDownloadQualityUrl(string streamUrl, string streamerId) {
            var processInfo = new ProcessStartInfo(GetYoutubeDlPath(), $"--dump-json {streamUrl}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            string json = process.StandardOutput.ReadLine();
            process.WaitForExit();

            Console.WriteLine(streamUrl);

            var deserializedJson = JsonConvert.DeserializeObject<YoutubeDlVideoJson.YoutubeDlVideo>(json);
            var returnValue = ParseBestPossibleQuality(deserializedJson, streamerId);


            returnValue.duration = deserializedJson.duration;
            returnValue.filename = deserializedJson._filename;

            Console.WriteLine(returnValue.quality);

            return returnValue;
        }

        public static YoutubeDlVideoJson.YoutubeDlVideoInfo ParseBestPossibleQuality(
            YoutubeDlVideoJson.YoutubeDlVideo deserializedJson, string streamerId) {
            var returnValue = new YoutubeDlVideoJson.YoutubeDlVideoInfo();

            List<SetupQualityExtendedJsonClass> availableQualities = new List<SetupQualityExtendedJsonClass>();
            for (var x = 0; x < deserializedJson.formats.Count; x++) {
                availableQualities.Add(new SetupQualityExtendedJsonClass {
                    Resolution = deserializedJson.formats[x].height,
                    Fps = RoundToNearest10(Convert.ToInt32(deserializedJson.formats[x].fps)),
                    tbr = deserializedJson.formats[x].tbr.Value
                });
            }

            // sort by highest quality first
            availableQualities = availableQualities.OrderByDescending(item => item.tbr).ToList();


            // saving for later, just in case
            /*for (var x = 0; x < deserializedJson.formats.Count; x++) {
                var splitRes = deserializedJson.formats[x].format_id.Split("p");
                availableQualities.Add(new SetupQualityExtendedJsonClass() {
                    Resolution = int.Parse(splitRes[0]),
                    Fps = int.Parse(splitRes[1]),
                    Counter = x
                });
            }*/

            Streamer streamerQuality;
            Config defaultQuality;
            using (var context = new DataContext()) {
                streamerQuality = context.Streamers.FirstOrDefault(item => item.streamerId == streamerId);
                defaultQuality = context.Configs.FirstOrDefault(item => item.key == "streamQuality");
            }

            int resolution = 0;
            double fps = 0;

            if (streamerQuality != null && streamerQuality.quality == null) {
                if (defaultQuality != null) {
                    var parsedQuality = JsonConvert.DeserializeObject<SetupQualityJsonClass>(defaultQuality.value);

                    resolution = parsedQuality.Resolution;
                    fps = parsedQuality.Fps;
                }
            } else {
                var parsedQuality = JsonConvert.DeserializeObject<SetupQualityJsonClass>(streamerQuality.quality);

                resolution = parsedQuality.Resolution;
                fps = parsedQuality.Fps;
            }


            if (resolution != 0 && fps != 0) {
                // check if the chosen resolution and fps is available
                var existingQuality =
                    availableQualities.FirstOrDefault(item => item.Resolution == resolution && item.Fps == fps);

                if (existingQuality != null) {
                    var selectedQuality =
                        deserializedJson.formats.FirstOrDefault(item => item.tbr == existingQuality.tbr);
                    if (selectedQuality != null) {
                        returnValue.url = selectedQuality.url;
                        returnValue.quality = selectedQuality.height;
                    }
                } else {
                    // get same resolution, but different fps (720p 60fps not available, maybe 720p 30fps?)
                    existingQuality = availableQualities.FirstOrDefault(item => item.Resolution == resolution);
                    if (existingQuality != null) {
                        var selectedQuality =
                            deserializedJson.formats.FirstOrDefault(item => item.tbr == existingQuality.tbr);
                        if (selectedQuality != null) {
                            returnValue.url = selectedQuality.url;
                            returnValue.quality = selectedQuality.height;
                        }
                    } else {
                        // same resolution and fps not available; choose the next best value (after sorting the list)
                        existingQuality = availableQualities.FirstOrDefault(item => item.Resolution < resolution);

                        if (existingQuality != null) {
                            var selectedQuality =
                                deserializedJson.formats.FirstOrDefault(item => item.tbr == existingQuality.tbr);
                            if (selectedQuality != null) {
                                returnValue.url = selectedQuality.url;
                                returnValue.quality = selectedQuality.height;
                            }
                        }
                    }
                }
            } else {
                returnValue.url = deserializedJson.url;
                returnValue.quality = deserializedJson.height;
            }

            return returnValue;
        }

        static int RoundToNearest10(int n) {
            int a = (n / 10) * 10;
            int b = a + 10;

            return (n - a > b - n) ? b : a;
        }

        private void SetDownloadToFinished(long streamId, bool isLive) {
            using (var context = new DataContext()) {
                var dbStream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                dbStream.size = new FileInfo(dbStream.downloadLocation).Length;
                dbStream.downloading = false;
                context.SaveChanges();

                if (isLive) {
                    Console.WriteLine("Stopping live chat download...");
                    BackgroundJob.Delete(dbStream.chatDownloadJobId);
                } else {
                    Console.WriteLine("Stopping VOD chat download.");
                }
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


        public string PrepareChat(long streamId) {
            string jobId = BackgroundJob.Enqueue(() => DownloadChat(streamId, CancellationToken.None));
            //_hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
            return jobId;
        }

        public string PrepareLiveChat(string channel, long streamId) {
            LiveStreamChatLogic liveStreamChatLogic = new LiveStreamChatLogic();
            string jobId =
                BackgroundJob.Enqueue(() => liveStreamChatLogic.DownloadLiveStreamChatLogic(channel, streamId, CancellationToken.None));

            return jobId;
        }

        [Queue("single")]
        public async Task DownloadChat(long streamId, CancellationToken token) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response =
                twitchApiHelpers.LegacyTwitchRequest($"https://api.twitch.tv/v5/videos/{streamId}/comments",
                    Method.GET);
            var deserializeResponse = JsonConvert.DeserializeObject<ChatMessageJsonClass.ChatMessage>(response.Content);
            ChatMessageJsonClass.ChatMessage chatMessage = new ChatMessageJsonClass.ChatMessage();
            chatMessage.comments = new List<ChatMessageJsonClass.Comment>();
            var cursor = "";
            int databaseCounter = 0;
            foreach (var comment in deserializeResponse.comments) {
                chatMessage.comments.Add(comment);
            }

            if (deserializeResponse._next != null) {
                cursor = deserializeResponse._next;
            } else {
                AddChatMessageToDb(chatMessage.comments, streamId);
            }

            while (cursor != null) {
                Console.WriteLine($"Getting more chat for {streamId}..");
                if (token.IsCancellationRequested) {
                    //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
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

                databaseCounter++;
                if (databaseCounter == 50 || deserializeResponse._next == null) {
                    // if we have collected 50 comments or there are no more chat messages
                    AddChatMessageToDb(chatMessage.comments, streamId);
                    databaseCounter = 0;
                    chatMessage.comments.Clear();
                }

                cursor = deserializeResponse._next;
            }

            SetChatDownloadToFinished(streamId);

            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
        }

        public void AddChatMessageToDb(List<ChatMessageJsonClass.Comment> comments, long streamId) {
            using (var context = new DataContext()) {
                Console.WriteLine("Saving chat...");
                foreach (var comment in comments) {
                    context.Chats.Add(new Chat {
                        streamId = streamId,
                        body = comment.message.body,
                        userId = comment.commenter._id,
                        userName = comment.commenter.name,
                        sentAt = comment.created_at,
                        offsetSeconds = comment.content_offset_seconds,
                        userBadges = JsonConvert.SerializeObject(comment.message.user_badges),
                        userColour = comment.message.user_color
                    });
                }

                context.SaveChanges();
            }
        }

        public void SetChatDownloadToFinished(long streamId) {
            Console.WriteLine("Chat finished downloading.");
            using (var context = new DataContext()) {
                var stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);

                if (stream != null) {
                    stream.chatDownloading = false;
                }

                context.SaveChanges();
            }
        }

        public class Data {
            public bool downloading { get; set; }
            public bool alreadyAdded { get; set; }
            public long size { get; set; }
            public string id { get; set; }
            public string user_id { get; set; }
            public string user_login { get; set; }
            public string user_name { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public DateTime created_at { get; set; }
            public DateTime started_at { get; set; }
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