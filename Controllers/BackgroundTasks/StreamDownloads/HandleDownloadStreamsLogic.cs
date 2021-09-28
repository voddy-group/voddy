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
using NLog;
using Quartz;
using Quartz.Impl;
using RestSharp;
using voddy.Controllers.BackgroundTasks.StreamDownloads;
using voddy.Controllers.BackgroundTasks.StreamDownloads.StreamDownloadJobs;
using voddy.Controllers.Streams;
using voddy.Controllers.Structures;
using voddy.Databases.Chat;
using voddy.Databases.Chat.Models;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Stream = voddy.Databases.Main.Models.Stream;


namespace voddy.Controllers {
    public class HandleDownloadStreamsLogic {
        private Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();

        public bool PrepareDownload(StreamExtended stream, bool isLive) {
            string streamUrl;
            string userLogin;
            using (var context = new MainDataContext()) {
                userLogin = context.Streamers
                    .FirstOrDefault(item => item.streamerId == stream.streamerId).username;
            }

            if (isLive) {
                streamUrl = "https://www.twitch.tv/" + userLogin;
            } else {
                streamUrl = "https://www.twitch.tv/videos/" + stream.streamId;
            }

            YoutubeDlVideoJson.YoutubeDlVideoInfo youtubeDlVideoInfo =
                GetDownloadQualityUrl(streamUrl, stream.streamerId);

            string streamDirectory = $"{GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}";


                    Directory.CreateDirectory(streamDirectory);

            if (!string.IsNullOrEmpty(stream.thumbnailLocation) && !isLive) {
                //todo handle missing thumbnail, maybe use youtubedl generated thumbnail instead
                DownloadHelpers downloadHelpers = new DownloadHelpers();
                downloadHelpers.DownloadFile(
                    stream.thumbnailLocation.Replace("%{width}", "320").Replace("%{height}", "180"),
                    $"{streamDirectory}/thumbnail.jpg");
            }

            string title = String.IsNullOrEmpty(stream.title) ? "vod" : stream.title;
            string outputPath = $"{streamDirectory}/{title}.{stream.streamId}";

            string dbOutputPath = $"streamers/{stream.streamerId}/vods/{stream.streamId}/{title}.{stream.streamId}.mp4";

            //TODO more should be queued, not done immediately

            IJobDetail job = JobBuilder.Create<DownloadStreamJob>()
                .WithIdentity("StreamDownload" + stream.streamId)
                .UsingJobData("title", title)
                .UsingJobData("streamDirectory", streamDirectory)
                .UsingJobData("youtubeDlVideoInfoUrl", youtubeDlVideoInfo.url)
                .UsingJobData("isLive", isLive)
                .UsingJobData("youtubeDlVideoInfoDuration", youtubeDlVideoInfo.duration)
                .RequestRecovery()
                .Build();

            job.JobDataMap.Put("stream", stream);
            var schedulerFactory = new StdSchedulerFactory(isLive ? QuartzSchedulers.RamScheduler() : QuartzSchedulers.PrimaryScheduler());
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.Start();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("StreamDownload" + stream.streamId)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);

            /*string jobId = BackgroundJob.Enqueue(() =>
                DownloadStream(stream, title, streamDirectory, youtubeDlVideoInfo.url, CancellationToken.None,
                    isLive, youtubeDlVideoInfo.duration));*/

            Stream? dbStream;

            using (var context = new MainDataContext()) {
                dbStream = context.Streams.FirstOrDefault(item => item.streamId == stream.streamId);

                if (dbStream != null) {
                    if (isLive) {
                        dbStream.vodId = stream.streamId;
                    } else {
                        dbStream.streamId = stream.streamId;
                    }

                    dbStream.streamerId = stream.streamerId;
                    dbStream.quality = youtubeDlVideoInfo.quality;
                    dbStream.url = youtubeDlVideoInfo.url;
                    dbStream.title = stream.title;
                    dbStream.createdAt = stream.createdAt;
                    dbStream.location = $"streamers/{stream.streamerId}/vods/{stream.streamId}/";
                    dbStream.fileName = $"{title}.{stream.streamId}.mp4";
                    dbStream.duration = youtubeDlVideoInfo.duration;
                    dbStream.downloading = true;
                    dbStream.downloadJobId = job.Key.ToString();
                    
                } else {
                    if (isLive) {
                        dbStream = new Stream {
                            vodId = stream.streamId,
                            streamerId = stream.streamerId,
                            quality = youtubeDlVideoInfo.quality,
                            title = stream.title,
                            url = youtubeDlVideoInfo.url,
                            createdAt = stream.createdAt,
                            location = $"streamers/{stream.streamerId}/vods/{stream.streamId}/",
                            fileName = $"{title}.{stream.streamId}.mp4",
                            downloading = true,
                            chatDownloading = true,
                            downloadJobId = job.Key.ToString(),
                            chatDownloadJobId = PrepareLiveChat(userLogin, stream.streamId).ToString()
                        };
                    } else {
                        IJobDetail chatDownloadJob = JobBuilder.Create<ChatDownloadJob>()
                            .WithIdentity("DownloadChat" + stream.streamId)
                            .UsingJobData("streamId", stream.streamId)
                            .RequestRecovery()
                            .Build();

                        ISimpleTrigger chatDownloadTrigger = (ISimpleTrigger)TriggerBuilder.Create()
                            .WithIdentity("DownloadChat" + stream.streamId)
                            .StartNow()
                            .Build();

                        scheduler.ScheduleJob(chatDownloadJob, chatDownloadTrigger);

                        dbStream = new Stream {
                            streamId = stream.streamId,
                            streamerId = stream.streamerId,
                            quality = youtubeDlVideoInfo.quality,
                            title = stream.title,
                            url = youtubeDlVideoInfo.url,
                            createdAt = stream.createdAt,
                            location = $"streamers/{stream.streamerId}/vods/{stream.streamId}/",
                            fileName = $"{title}.{stream.streamId}.mp4",
                            duration = youtubeDlVideoInfo.duration,
                            downloading = true,
                            chatDownloading = true,
                            downloadJobId = job.Key.ToString(),
                            chatDownloadJobId = chatDownloadJob.Key.ToString()
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

        public bool DownloadSingleStream(long streamId, HandleDownloadStreamsLogic.Data liveStream) {
            using (var context = new MainDataContext()) {
                var existingStream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                if (existingStream != null) {
                    // stream already exists in database
                    return false;
                }
            }

            StreamExtended stream;

            if (liveStream != null) {
                stream = new StreamExtended {
                    streamerId = liveStream.user_id,
                    streamId = streamId,
                    thumbnailLocation = liveStream.thumbnail_url,
                    title = liveStream.title,
                    createdAt = liveStream.started_at
                };
            } else {
                TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
                var response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                              $"?id={streamId}", Method.GET);
                var deserializeResponse =
                    JsonConvert.DeserializeObject<GetStreamsResult>(response.Content);
                stream = new StreamExtended {
                    streamerId = deserializeResponse.data[0].user_id,
                    streamId = streamId,
                    thumbnailLocation = deserializeResponse.data[0].thumbnail_url,
                    title = deserializeResponse.data[0].title,
                    createdAt = deserializeResponse.data[0].created_at
                };
            }

            return PrepareDownload(stream, liveStream != null);
        }

        public void DownloadAllStreams(int streamerId) {
            GetStreamLogic getStreamLogic = new GetStreamLogic();
            var streams = getStreamLogic.GetStreamsWithFiltersLogic(streamerId).Where(item => item.id != -1);
            foreach (var stream in streams) {
                PrepareDownload(stream, false);
            }
        }

        public string CheckForDownloadingStreams(bool skip = false) {
            int currentlyDownloading;
            using (var context = new MainDataContext()) {
                currentlyDownloading =
                    context.Streams.Count(item => item.downloading || item.chatDownloading);
            }

            if (currentlyDownloading > 0) {
                return $"Downloading {currentlyDownloading} streams/chats...";
            }

            return "";
        }

        [Queue("default")]
        public async Task DownloadStream(StreamExtended stream, string title, string streamDirectory, string url,
            bool isLive, long duration, CancellationToken? cancellationToken) {
            string youtubeDlPath = GetYoutubeDlPath();

            var processInfo =
                new ProcessStartInfo(youtubeDlPath, $"{url} -o \"{streamDirectory}/{title}.{stream.streamId}.mp4\"");
            Console.WriteLine($"{url} -o \"{streamDirectory}/{title}.{stream.streamId}.mp4\"");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                Console.WriteLine("output>>" + e.Data);
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) {
                    process.Kill(); // insta kill
                    process.WaitForExit();
                }
            };
            process.BeginOutputReadLine();


            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                Console.WriteLine("error>>" + e.Data);
                /*if (token.IsCancellationRequested) {
                    process.Kill(); // insta kill
                    process.WaitForExit();
                } else {*/
                if (!isLive && e.Data != null) {
                    GetProgress(e.Data, stream.streamId, duration);
                }
                // }
            };
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            _logger.Info(isLive
                ? "Stream has gone offline, stopped downloading."
                : "VOD downloaded, stopped downloading");

            SetDownloadToFinished(stream.streamId, isLive);
            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
        }

        public bool CheckIfLiveStreamRequeued(StreamExtended stream, bool isLive) {
            if (stream.createdAt.Ticks != 0 && isLive && DateTime.UtcNow.Subtract(stream.createdAt).TotalMinutes > 5) {
                _logger.Error("Attempted to record live stream past 5 minute mark! Cancelling download.");
                DeleteStreamsLogic deleteStreamsLogic = new DeleteStreamsLogic();
                deleteStreamsLogic.DeleteSingleStreamLogic(stream.id);
                BackgroundJob.Delete(stream.chatDownloadJobId);
                return false;
            }

            return true;
        }

        public void GetProgress(string line, long streamId, long duration) {
            var splitString = line.Split(" ");
            var time = splitString.FirstOrDefault(item => item.StartsWith("time="));
            if (time != null) {
                while (true) {
                    TimeSpan parsed;
                    try {
                        parsed = TimeSpan.Parse(time.Replace("time=", ""));
                    } catch (FormatException) {
                        // cannot parse; just move on
                        break;
                    }

                    TimeSpan vodDuration = TimeSpan.FromSeconds(duration);
                    NotificationHub.Current.Clients.All.SendAsync($"{streamId}-progress",
                        ((double)parsed.Ticks / (double)vodDuration.Ticks) * 100);
                    break;
                }
            }
        }

        private YoutubeDlVideoJson.YoutubeDlVideoInfo
            GetDownloadQualityUrl(string streamUrl, int streamerId) {
            var processInfo = new ProcessStartInfo(GetYoutubeDlPath(), $"--dump-json {streamUrl}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            string json = process.StandardOutput.ReadLine();
            process.WaitForExit();


            var deserializedJson = JsonConvert.DeserializeObject<YoutubeDlVideoJson.YoutubeDlVideo>(json ??
                throw new Exception(
                    "Cannot download stream/vod. May be offline (slow updating twitch api) or vod is no longer available."));
            var returnValue = ParseBestPossibleQuality(deserializedJson, streamerId);


            returnValue.duration = deserializedJson.duration;
            returnValue.filename = deserializedJson._filename;


            return returnValue;
        }

        public static YoutubeDlVideoJson.YoutubeDlVideoInfo ParseBestPossibleQuality(
            YoutubeDlVideoJson.YoutubeDlVideo deserializedJson, int streamerId) {
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
            string defaultQuality = GlobalConfig.GetGlobalConfig("streamQuality");
            using (var context = new MainDataContext()) {
                streamerQuality = context.Streamers.FirstOrDefault(item => item.streamerId == streamerId);
            }

            int resolution = 0;
            double fps = 0;

            if (streamerQuality != null && streamerQuality.quality == null) {
                if (defaultQuality != null) {
                    var parsedQuality = JsonConvert.DeserializeObject<SetupQualityJsonClass>(defaultQuality);

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
            using (var context = new MainDataContext()) {
                Stream dbStream;
                
                if (isLive) {
                    dbStream = context.Streams.FirstOrDefault(item => item.vodId == streamId);
                } else {
                    dbStream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                }

                string streamFile = GlobalConfig.GetGlobalConfig("contentRootPath") + dbStream.location + dbStream.fileName;
                dbStream.size = new FileInfo(streamFile).Length;
                dbStream.downloading = false;

                NotificationHub.Current.Clients.All.SendAsync($"{streamId}-completed",
                    dbStream);

                if (isLive) {
                    _logger.Info("Stopping live chat download...");
                    if (dbStream.chatDownloadJobId.Contains(".")) {
                        var splitJobKey = dbStream.chatDownloadJobId.Split(".");
                        JobHelpers.CancelJob(splitJobKey[1], splitJobKey[0], QuartzSchedulers.PrimaryScheduler());
                    } else {
                        JobHelpers.CancelJob(dbStream.chatDownloadJobId, null,
                            QuartzSchedulers.PrimaryScheduler());
                    }
                    dbStream.chatDownloading = false;
                    dbStream.duration = getStreamDuration(streamFile);
                    LiveStreamEndJobs(streamId);
                } else {
                    _logger.Info("Stopping VOD chat download.");
                }

                context.SaveChanges();

                // make another background job for this
                string checkVideoThumbnailsEnabled = GlobalConfig.GetGlobalConfig("generateVideoThumbnails");

                if (checkVideoThumbnailsEnabled != null && checkVideoThumbnailsEnabled == "True") {
                    IJobDetail job = JobBuilder.Create<GenerateVideoThumbnailJob>()
                        .WithIdentity("GenerateVideoThumbnail" + streamId)
                        .UsingJobData("streamId", streamId)
                        .UsingJobData("streamFile", streamFile)
                        .Build();

                    var schedulerFactory = new StdSchedulerFactory(QuartzSchedulers.PrimaryScheduler());
                    IScheduler scheduler = schedulerFactory.GetScheduler().Result;
                    scheduler.Start();

                    ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                        .WithIdentity("GenerateVideoThumbnail" + streamId)
                        .StartNow()
                        .Build();

                    scheduler.ScheduleJob(job, trigger);
                    //BackgroundJob.Enqueue(() => GenerateVideoThumbnail(streamId, streamFile));
                }
            }
        }

        private int getStreamDuration(string streamFile) {
            Task<IMediaInfo> streamFileInfo = FFmpeg.GetMediaInfo(streamFile);
            return (int)streamFileInfo.Result.Duration.TotalSeconds;
        }

        public void GenerateVideoThumbnail(long streamId, string streamFile) {
            Stream stream;
            using (var context = new MainDataContext()) {
                stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
            }

            Task<IMediaInfo> streamFileInfo = FFmpeg.GetMediaInfo(streamFile);

            double segment = streamFileInfo.Result.Duration.TotalSeconds / 5;
            double duration = 0;
            List<string> fileNames = new List<string>();

            for (var x = 0; x < 5; x++) {
                string fileOutput =
                    $"{GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}/thumbnailVideo-{x}.mp4";
                IConversion conversion = FFmpeg.Conversions.New()
                    .AddStream(streamFileInfo.Result.VideoStreams.FirstOrDefault()?.SetSize(320, 180))
                    .AddParameter($"-ss {duration + segment - 5} -an -t 5")
                    .SetPriority(ProcessPriorityClass.BelowNormal)
                    .SetPreset(ConversionPreset.UltraFast)
                    .SetOverwriteOutput(true)
                    .SetOutput(fileOutput);
                conversion.Start().Wait();
                duration = duration + segment;
                fileNames.Add(fileOutput);
            }

            using (StreamWriter outputFile =
                new StreamWriter(
                    $"{GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}/thumbnailVideoConcat.txt")) {
                for (int x = 0; x < fileNames.Count; x++) {
                    outputFile.WriteLine($"file '{fileNames[x]}'");
                }
            }

            IConversion concatConversion = FFmpeg.Conversions.New()
                .AddParameter(
                    $"-f concat -safe 0 -i {GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}/thumbnailVideoConcat.txt -c copy {GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}/thumbnailVideo.mp4");

            concatConversion.Start().Wait();

            using (var context = new MainDataContext()) {
                stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                stream.hasVideoThumbnail = true;
                context.SaveChanges();
            }

            // cleanup
            for (int x = 0; x < fileNames.Count; x++) {
                FileInfo seperateFile = new FileInfo(fileNames[x]);

                seperateFile.Delete();
            }

            new FileInfo(
                    $"{GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}/thumbnailVideoConcat.txt")
                .Delete();
        }

        private async void LiveStreamEndJobs(long streamId) {
            ChangeStreamIdToVodId(streamId);
            await GenerateThumbnailDuration(streamId);
        }

        private async Task GenerateThumbnailDuration(long vodId) {
            try {
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            } catch (NotImplementedException) {
                _logger.Warn("OS not supported. Skipping thumbnail generation.");
                return;
            }

            Stream stream;
            using (var context = new MainDataContext()) {
                stream = context.Streams.FirstOrDefault(item => item.vodId == vodId);

                if (stream != null) {
                    stream.duration = FFmpeg.GetMediaInfo(GlobalConfig.GetGlobalConfig("contentRootPath") + stream.location + stream.fileName).Result
                        .Duration
                        .Seconds;
                }

                context.SaveChanges();
            }

            if (stream != null) {
                Task<IMediaInfo> streamFile =
                    FFmpeg.GetMediaInfo(Path.Combine(GlobalConfig.GetGlobalConfig("contentRootPath"), stream.location, stream.fileName));
                var conversion = await FFmpeg.Conversions.New()
                    .AddStream(streamFile.Result.Streams.FirstOrDefault())
                    .AddParameter("-vframes 1 -s 320x180")
                    .SetOutput(Path.Combine(GlobalConfig.GetGlobalConfig("contentRootPath"), stream.location, "thumbnail.jpg"))
                    .Start();
            }
        }

        private void ChangeStreamIdToVodId(long streamId) {
            long streamerId = 0;
            Stream stream;
            using (var context = new MainDataContext()) {
                stream = context.Streams.FirstOrDefault(item => item.vodId == streamId);
            }

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            if (stream != null) {
                var response =
                    twitchApiHelpers.TwitchRequest(
                        $"https://api.twitch.tv/helix/videos?user_id={stream.streamerId}&first=1", Method.GET);

                var deserializedJson = JsonConvert.DeserializeObject<GetStreamsResult>(response.Content);

                if (deserializedJson.data[0] != null) {
                    var streamVod = deserializedJson.data[0];

                    if ((streamVod.created_at - stream.createdAt).TotalSeconds < 20) {
                        // if stream was created within 20 seconds of going live. Not very reliable but is the only way I can see how to implement it.
                        using (var context = new MainDataContext()) {
                            stream = context.Streams.FirstOrDefault(item => item.vodId == streamId);
                            stream.streamId = Int64.Parse(streamVod.id);
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

        private static string GetYoutubeDlPath() {
            string youtubeDlInstance = GlobalConfig.GetGlobalConfig("youtube-dl");
            if (youtubeDlInstance != null) {
                return youtubeDlInstance;
            }

            return "youtube-dl";
        }

        public JobKey PrepareLiveChat(string channel, long streamId) {
            IJobDetail job = JobBuilder.Create<LiveStreamChatDownloadJob>()
                .WithIdentity("LiveStreamDownloadJob" + streamId)
                .UsingJobData("channel", channel)
                .UsingJobData("streamId", streamId)
                .Build();

            var schedulerFactory = new StdSchedulerFactory(QuartzSchedulers.RamScheduler());
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.Start();
            
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("LiveStreamDownloadTrigger" + streamId)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);
            /*LiveStreamChatLogic liveStreamChatLogic = new LiveStreamChatLogic();
            string jobId =
                BackgroundJob.Enqueue(() =>
                    liveStreamChatLogic.DownloadLiveStreamChatLogic(channel, streamId, CancellationToken.None));
            */
            return job.Key;
        }

        [Queue("single")]
        public async Task DownloadChat(long streamId) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response =
                twitchApiHelpers.LegacyTwitchRequest($"https://api.twitch.tv/v5/videos/{streamId}/comments",
                    Method.GET);
            var deserializeResponse = JsonConvert.DeserializeObject<ChatMessageJsonClass.ChatMessage>(response.Content);
            ChatMessageJsonClass.ChatMessage chatMessage = new ChatMessageJsonClass.ChatMessage();
            chatMessage.comments = new List<ChatMessageJsonClass.Comment>();
            var cursor = "";
            int databaseCounter = 0;
            // clear out existing vod messages, should only activate when redownloading
            using (var context = new ChatDataContext()) {
                var existingChat = context.Chats.Where(item => item.streamId == streamId);
                context.RemoveRange(existingChat);
                context.SaveChanges();
            }

            foreach (var comment in deserializeResponse.comments) {
                if (comment.message.user_badges != null) {
                    comment.message.userBadges = ReformatBadges(comment.message.user_badges);
                }

                chatMessage.comments.Add(comment);
            }

            if (deserializeResponse._next != null) {
                cursor = deserializeResponse._next;
            } else {
                AddChatMessageToDb(chatMessage.comments, streamId);
            }

            while (cursor != null) {
                _logger.Info($"Getting more chat for {streamId}..");
                //if (token.IsCancellationRequested) {
                    //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
                    //return; // insta kill 
                //}

                var paginatedResponse = twitchApiHelpers.LegacyTwitchRequest(
                    $"https://api.twitch.tv/v5/videos/{streamId}/comments" +
                    $"?cursor={deserializeResponse._next}", Method.GET);
                deserializeResponse =
                    JsonConvert.DeserializeObject<ChatMessageJsonClass.ChatMessage>(paginatedResponse.Content);
                foreach (var comment in deserializeResponse.comments) {
                    if (comment.message.user_badges != null) {
                        comment.message.userBadges = ReformatBadges(comment.message.user_badges);
                    }

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

            SetChatDownloadToFinished(streamId, false);

            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
        }

        private string ReformatBadges(List<ChatMessageJsonClass.UserBadge> userBadges) {
            string reformattedBadges = "";
            for (int x = 0; x < userBadges.Count; x++) {
                if (x != userBadges.Count - 1) {
                    reformattedBadges += $"{userBadges[x]._id}:{userBadges[x].version},";
                } else {
                    reformattedBadges += $"{userBadges[x]._id}:{userBadges[x].version}";
                }
            }

            return reformattedBadges;
        }

        public void AddChatMessageToDb(List<ChatMessageJsonClass.Comment> comments, long streamId) {
            using (var context = new ChatDataContext()) {
                _logger.Info("Saving chat...");
                foreach (var comment in comments) {
                    context.Chats.Add(new Chat {
                        messageId = comment._id,
                        streamId = streamId,
                        body = comment.message.body,
                        userId = comment.commenter._id,
                        userName = comment.commenter.name,
                        sentAt = comment.created_at,
                        offsetSeconds = comment.content_offset_seconds,
                        userBadges = comment.message.userBadges,
                        userColour = comment.message.user_color
                    });
                }

                context.SaveChanges();
            }
        }

        public void SetChatDownloadToFinished(long streamId, bool isLive) {
            _logger.Info("Chat finished downloading.");
            using (var context = new MainDataContext()) {
                Stream stream;
                if (isLive) {
                    stream = context.Streams.FirstOrDefault(item => item.vodId == streamId);
                } else {
                    stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                }

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
            public int user_id { get; set; }
            public string user_login { get; set; }
            public string user_name { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public DateTime created_at { get; set; }
            public DateTime started_at { get; set; }
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