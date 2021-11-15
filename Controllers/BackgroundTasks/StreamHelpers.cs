using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using Quartz;
using Quartz.Impl;
using RestSharp;
using voddy.Controllers.BackgroundTasks.StreamDownloads;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Stream = voddy.Databases.Main.Models.Stream;

namespace voddy.Controllers.BackgroundTasks {
    public class StreamHelpers {
        public static StreamExtended GetStreamDetails(long streamId, bool isLive = false, long userId = default) {
            Logger _logger = new LogFactory().GetCurrentClassLogger();

            using (var context = new MainDataContext()) {
                var existingStream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                if (existingStream != null) {
                    // stream already exists in database
                    throw new Exception("Stream already exists.");
                }
            }

            StreamExtended stream;

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            IRestResponse response;
            try {
                if (isLive) {
                    response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                              $"?user_id={userId}", Method.GET);
                } else {
                    response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/videos" +
                                                              $"?id={streamId}", Method.GET);
                }
            } catch (NetworkInformationException e) {
                _logger.Error(e);
                throw;
            }

            var deserializeResponse =
                JsonConvert.DeserializeObject<GetStreamsResult>(response.Content);

            StreamExtended retrievedStream;

            if (isLive) {
                var tempStream = deserializeResponse.data.First(item => Int64.Parse(item.stream_id) == streamId);
                return new StreamExtended {
                    streamId = Int32.Parse(tempStream.id),
                    started_at = tempStream.started_at,
                    vodId = streamId,
                    streamerId = tempStream.user_id,
                    title = tempStream.title,
                    createdAt = tempStream.created_at
                };
            } else {
                return new StreamExtended {
                    streamerId = deserializeResponse.data[0].user_id,
                    streamId = streamId,
                    thumbnailLocation = deserializeResponse.data[0].thumbnail_url,
                    title = deserializeResponse.data[0].title,
                    createdAt = deserializeResponse.data[0].created_at
                };
            }
        }

        public static YoutubeDlVideoJson.YoutubeDlVideoInfo
            GetDownloadQualityUrl(string streamUrl, int streamerId) {
            var processInfo = new ProcessStartInfo(GetYtDlpPath(), $"--dump-json {streamUrl}");
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
            returnValue.formatId = deserializedJson.format_id;


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

        private static int RoundToNearest10(int n) {
            int a = (n / 10) * 10;
            int b = a + 10;

            return (n - a > b - n) ? b : a;
        }

        public static string GetYtDlpPath() {
            string ytDlpInstance = GlobalConfig.GetGlobalConfig("yt-dlp");
            if (ytDlpInstance != null) {
                return ytDlpInstance;
            }

            return "yt-dlp";
        }

        public static void SetDownloadToFinished(long streamId, bool isLive) {
            Logger _logger = new NLog.LogFactory().GetCurrentClassLogger();

            using (var context = new MainDataContext()) {
                Stream dbStream;

                dbStream = context.Streams.FirstOrDefault(item => item.streamId == streamId);

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
                    GenerateThumbnailDuration(streamId);
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

        private static int getStreamDuration(string streamFile) {
            Task<IMediaInfo> streamFileInfo = FFmpeg.GetMediaInfo(streamFile);
            return (int)streamFileInfo.Result.Duration.TotalSeconds;
        }

        private static async void LiveStreamEndJobs(long streamId) {
            //ChangeStreamIdToVodId(streamId);
            await GenerateThumbnailDuration(streamId);
        }

        private static async Task GenerateThumbnailDuration(long vodId) {
            Logger _logger = new NLog.LogFactory().GetCurrentClassLogger();

            try {
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            } catch (NotImplementedException) {
                _logger.Warn("OS not supported. Skipping thumbnail generation.");
                return;
            }

            Stream stream;
            using (var context = new MainDataContext()) {
                stream = context.Streams.FirstOrDefault(item => item.streamId == vodId);

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

        private static void ChangeStreamIdToVodId(long streamId) {
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

        public static void SetChatDownloadToFinished(long streamId, bool isLive) {
            Logger _logger = new NLog.LogFactory().GetCurrentClassLogger();

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
            public string stream_id { get; set; }
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