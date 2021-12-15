#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using Quartz;
using Quartz.Impl;
using RestSharp;
using voddy.Controllers.BackgroundTasks.LiveStreamDownloads;
using voddy.Controllers.BackgroundTasks.StreamDownloads.StreamDownloadJobs;
using voddy.Controllers.Notifications;
using voddy.Controllers.Streams;
using voddy.Controllers.Structures;
using voddy.Databases.Chat;
using voddy.Databases.Chat.Models;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;
using voddy.Exceptions.Streams;
using Xabe.FFmpeg;
using Position = voddy.Databases.Main.Models.Position;
using Stream = voddy.Databases.Main.Models.Stream;


namespace voddy.Controllers.BackgroundTasks.StreamDownloads {
    public class HandleDownloadStreamsLogic {
        private Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();

        public bool PrepareDownload(StreamExtended stream) {
            string streamUrl;

            streamUrl = "https://www.twitch.tv/videos/" + stream.streamId;

            YoutubeDlVideoJson.YoutubeDlVideoInfo youtubeDlVideoInfo =
                StreamHelpers.GetDownloadQualityUrl(streamUrl, stream.streamerId);

            string streamDirectory = $"{GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}";


            Directory.CreateDirectory(streamDirectory);

            if (!string.IsNullOrEmpty(stream.thumbnailLocation)) {
                //todo handle missing thumbnail, maybe use youtubedl generated thumbnail instead
                DownloadHelpers.DownloadFile(
                    stream.thumbnailLocation.Replace("%{width}", "320").Replace("%{height}", "180"),
                    $"{streamDirectory}/thumbnail.jpg");
            }

            string title = String.IsNullOrEmpty(stream.title) ? "vod" : stream.title;
            string outputPath = $"{streamDirectory}/{title}.{stream.streamId}";

            string dbOutputPath = $"streamers/{stream.streamerId}/vods/{stream.streamId}/{title}.{stream.streamId}.mp4";

            //TODO more should be queued, not done immediately


            IJobDetail job;
            string triggerIdentity;
            job = JobBuilder.Create<DownloadStreamJob>()
                .WithIdentity("StreamDownload" + stream.streamId)
                .UsingJobData("title", title)
                .UsingJobData("streamDirectory", streamDirectory)
                .UsingJobData("formatId", youtubeDlVideoInfo.formatId)
                .UsingJobData("url", streamUrl)
                .UsingJobData("isLive", false)
                .UsingJobData("youtubeDlVideoInfoDuration", youtubeDlVideoInfo.duration)
                .UsingJobData("retry", true)
                .RequestRecovery()
                .Build();

            job.JobDataMap.Put("stream", stream);
            triggerIdentity = $"StreamDownload{stream.streamId}";


            /*string jobId = BackgroundJob.Enqueue(() =>
                DownloadStream(stream, title, streamDirectory, youtubeDlVideoInfo.url, CancellationToken.None,
                    isLive, youtubeDlVideoInfo.duration));*/

            Stream? dbStream;
            bool downloadChat = false;
            IJobDetail chatDownloadJob = new JobDetailImpl();

            using (var context = new MainDataContext()) {
                dbStream = context.Streams.FirstOrDefault(item => item.streamId == stream.streamId);

                if (dbStream != null) {
                    dbStream.streamId = stream.streamId;

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
                    downloadChat = true;
                    chatDownloadJob = JobBuilder.Create<ChatDownloadJob>()
                        .WithIdentity("DownloadChat" + stream.streamId)
                        .UsingJobData("streamId", stream.streamId)
                        .UsingJobData("retry", true)
                        .RequestRecovery()
                        .Build();

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
                    // only download chat if this is a new vod


                    context.Add(dbStream);
                }

                context.SaveChanges();
            }

            //var chatSchedulerFactory = new StdSchedulerFactory(QuartzSchedulers.SingleThreadScheduler());
            var vodSchedulerFactory = new StdSchedulerFactory(QuartzSchedulers.PrimaryScheduler());
            //IScheduler chatScheduler = chatSchedulerFactory.GetScheduler().Result;
            IScheduler vodScheduler = vodSchedulerFactory.GetScheduler().Result;
            //chatScheduler.Start();
            vodScheduler.Start();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity(triggerIdentity)
                .StartNow()
                .Build();

            vodScheduler.ScheduleJob(job, trigger);

            if (downloadChat) {
                ISimpleTrigger chatDownloadTrigger = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity("DownloadChat" + stream.streamId)
                    .StartNow()
                    .Build();

                vodScheduler.ScheduleJob(chatDownloadJob, chatDownloadTrigger);
            }

            //_hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());

            return true;
        }


        public void DownloadAllStreams(int streamerId) {
            GetStreamLogic getStreamLogic = new GetStreamLogic();
            var streams = getStreamLogic.GetStreamsWithFiltersLogic(streamerId).Where(item => item.id != -1);
            foreach (var stream in streams) {
                PrepareDownload(stream);
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

        public Task DownloadStream(StreamExtended stream, string title, string streamDirectory, string formatId,
            string url, long duration, CancellationToken cancellationToken) {
            StreamDownload streamDownload = new StreamDownload(new DirectoryInfo(streamDirectory), false);
            _logger.Info("Getting vod m3u8..");
            streamDownload.GetVodM3U8(url);
            _logger.Info("Got vod m3u8.");
            
            try {
                _logger.Info("Getting vod parts...");
                streamDownload.GetVodParts(cancellationToken);
                _logger.Info("Got vod parts.");
            } catch (TsFileNotFound e) {
                // test if the stream has gone down.
                try {
                    streamDownload.GetVodM3U8(url);
                } catch (Exception exception) {
                    if (!exception.Message.Contains("is offline")) {
                        // stream is offline, must have finished, so is not an error.
                        // stream has not finished, throw
                        _logger.Error($"Error occured while downloading a live stream. {exception.Message}");
                        Streamer streamer;
                        using (var mainDataContext = new MainDataContext()) {
                            streamer = mainDataContext.Streamers.FirstOrDefault(item => item.streamerId == stream.streamerId);
                        }

                        if (streamer != null) {
                            NotificationLogic.CreateNotification($"StreamDownloadJob{stream.streamId}", Severity.Error, Position.Top, $"Could not download VOD for {streamer.username}.", $"/streamer/{streamer.id}");
                        }

                        streamDownload.CleanUpFiles();
                        throw;
                    }
                }
            }
            
            _logger.Info("Combining ts files...");
            streamDownload.CombineTsFiles(title, stream.streamId);
            _logger.Info("Combined ts files.");
            _logger.Info("Cleaning up files...");
            streamDownload.CleanUpFiles();
            _logger.Info("Cleaned up files.");
            
            File.Move($"{streamDownload._rootDirectory.FullName}/stream.mp4", $"{streamDownload._rootDirectory.FullName}/{title}.{stream.streamId}.mp4");
            
            _logger.Info("Moved file.");
            StreamHelpers.SetDownloadToFinished(stream.streamId, false);
            return Task.CompletedTask;
            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
        }


        public void GetProgress(string line, long streamId) {
            if (line != null) {
                var splitString = line.Split(" ");
                string percentage = splitString.FirstOrDefault(item => item.EndsWith("%"));
                if (percentage != null) {
                    NotificationHub.Current.Clients.All.SendAsync($"{streamId}-progress",
                        percentage);
                }
            }
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

        public Task DownloadChat(long streamId) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            IRestResponse response;
            try {
                response =
                    twitchApiHelpers.LegacyTwitchRequest($"https://api.twitch.tv/v5/videos/{streamId}/comments",
                        Method.GET);
            } catch (NetworkInformationException e) {
                _logger.Error(e);
                _logger.Error("Cleaning database, removing failed chat download from database.");
                RemoveStreamChatFromDb(streamId);
                return Task.FromException(e);
            }

            ChatMessageJsonClass.ChatMessage deserializeResponse;
            try {
                deserializeResponse = JsonConvert.DeserializeObject<ChatMessageJsonClass.ChatMessage>(response.Content);
            } catch (JsonSerializationException e) {
                _logger.Error(e);
                _logger.Error("Cleaning database, removing failed chat download from database.");
                RemoveStreamChatFromDb(streamId);
                return Task.FromException(e);
            }
            
            ChatMessageJsonClass.ChatMessage chatMessage = new ChatMessageJsonClass.ChatMessage();
            chatMessage.comments = new List<ChatMessageJsonClass.Comment>();
            var cursor = "";
            int databaseCounter = 0;
            // clear out existing vod messages, should only activate when redownloading
            RemoveStreamChatFromDb(streamId);

            foreach (var comment in deserializeResponse.comments) {
                if (comment.message.user_badges != null) {
                    comment.message.userBadges = ReformatBadges(comment.message.user_badges);
                }

                if (comment.message.emoticons != null) {
                    comment.message.formattedEmoticons = ReformatEmoticons(comment.message.emoticons);
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

                IRestResponse paginatedResponse;
                try {
                    paginatedResponse = twitchApiHelpers.LegacyTwitchRequest(
                        $"https://api.twitch.tv/v5/videos/{streamId}/comments" +
                        $"?cursor={deserializeResponse._next}", Method.GET);
                } catch (NetworkInformationException e) {
                    _logger.Error(e);
                    _logger.Error("Cleaning database, removing failed chat download from database.");
                    RemoveStreamChatFromDb(streamId);
                    return Task.FromException(e);
                }

                try {
                    deserializeResponse = JsonConvert.DeserializeObject<ChatMessageJsonClass.ChatMessage>(paginatedResponse.Content);
                } catch (JsonSerializationException e) {
                    _logger.Error(e);
                    _logger.Error("Cleaning database, removing failed chat download from database.");
                    RemoveStreamChatFromDb(streamId);
                    return Task.FromException(e);
                }
                
                foreach (var comment in deserializeResponse.comments) {
                    if (comment.message.user_badges != null) {
                        comment.message.userBadges = ReformatBadges(comment.message.user_badges);
                    }

                    if (comment.message.emoticons != null) {
                        comment.message.formattedEmoticons = ReformatEmoticons(comment.message.emoticons);
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

            StreamHelpers.SetChatDownloadToFinished(streamId, false);

            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", CheckForDownloadingStreams());
            return Task.CompletedTask;
        }

        private void RemoveStreamChatFromDb(long streamId) {
            using (var context = new ChatDataContext()) {
                var existingChat = context.Chats.Where(item => item.streamId == streamId);
                context.RemoveRange(existingChat);
                context.SaveChanges();
            }
        }

        private string ReformatBadges(List<ChatMessageJsonClass.UserBadge> userBadges) {
            string reformattedBadges = "";
            for (int x = 0; x < userBadges.Count; x++) {
                if (x != userBadges.Count - 1) {
                    reformattedBadges += $"{userBadges[x]._id}:{userBadges[x].version}/";
                } else {
                    reformattedBadges += $"{userBadges[x]._id}:{userBadges[x].version}";
                }
            }

            return reformattedBadges;
        }

        public string ReformatEmoticons(List<ChatMessageJsonClass.Emoticons> emoticonsList) {
            Dictionary<string, string> dbEmoticons = new();
            for (int i = 0; i < emoticonsList.Count; i++) {
                if (!dbEmoticons.ContainsKey(emoticonsList[i]._id)) {
                    List<ChatMessageJsonClass.Emoticons> totalOccurrencesOfEmote = emoticonsList.Where(emote => emote._id == emoticonsList[i]._id).ToList();
                    List<string> listOfStartEndIndexes = new();
                    for (int j = 0; j < totalOccurrencesOfEmote.Count; j++) {
                        listOfStartEndIndexes.Add($"{totalOccurrencesOfEmote[j].begin}-{totalOccurrencesOfEmote[j].end}");
                    }

                    dbEmoticons.Add(emoticonsList[i]._id, String.Join(",", listOfStartEndIndexes));
                }
            }

            return String.Join("/", dbEmoticons.Select(emoticon => $"{emoticon.Key}:{emoticon.Value}"));
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
                        userColour = comment.message.user_color,
                        mod = comment.message.userBadges?.Contains("moderator") ?? false,
                        subscriber = comment.message.userBadges?.Contains("subscriber") ?? false,
                        turbo = comment.message.userBadges?.Contains("turbo") ?? false,
                        emotes = comment.message.formattedEmoticons
                    });
                }

                context.SaveChanges();
            }
        }
    }
}