using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Quartz.Impl;
using voddy.Controllers.BackgroundTasks.LiveStreamDownloads.LiveStreamDownloadJobs;
using voddy.Controllers.BackgroundTasks.StreamDownloads.StreamDownloadJobs;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using Stream = voddy.Databases.Main.Models.Stream;

namespace voddy.Controllers.BackgroundTasks.LiveStreamDownloads {
    public class CreateLiveStream {
        private Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();
        private string streamUrl { get; set; }
        private string streamDirectory { get; set; }
        private string outputPath { get; set; }
        private string dbOutputPath { get; set; }
        
        public Task PrepareLiveStreamDownload(StreamExtended stream, string streamerName) {
            streamUrl = "https://twitch.tv/" + streamerName;

            YoutubeDlVideoJson.YoutubeDlVideoInfo youtubeDlVideoInfo = StreamHelpers.GetDownloadQualityUrl(streamUrl, stream.streamerId);
            streamDirectory = $"{GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{stream.streamerId}/vods/{stream.streamId}";

            try {
                Directory.CreateDirectory(streamDirectory);
            } catch (UnauthorizedAccessException e) {
                _logger.Error(e);
                // todo handle this
                throw;
            }
            
            outputPath = $"{streamDirectory}/{stream.title}.{stream.streamId}";
            
            dbOutputPath = $"streamers/{stream.streamerId}/vods/{stream.streamId}/{stream.title}.{stream.streamId}.mp4";
            
            var job = JobBuilder.Create<LiveStreamDownloadJob>()
                .UsingJobData("url", streamUrl)
                .UsingJobData("streamDirectory", streamDirectory)
                .UsingJobData("streamId", stream.streamId)
                .UsingJobData("title", stream.title)
                .Build();
            
            var triggerIdentity = $"LiveStreamDownload{stream.streamId}";
            
            var chatDownloadJob = JobBuilder.Create<LiveStreamChatDownloadJob>()
                .WithIdentity("LiveStreamChatDownloadJob" + stream.streamId)
                .UsingJobData("channel", streamerName)
                .UsingJobData("streamId", stream.streamId)
                .Build();

            using (var context = new MainDataContext()) {
                var dbStream = new Stream {
                    streamId = stream.streamId,
                    vodId = stream.vodId,
                    streamerId = stream.streamerId,
                    quality = youtubeDlVideoInfo.quality,
                    title = stream.title,
                    url = youtubeDlVideoInfo.url,
                    createdAt = stream.createdAt,
                    location = $"streamers/{stream.streamerId}/vods/{stream.streamId}/",
                    fileName = $"{stream.title}.{stream.streamId}.mp4",
                    downloading = true,
                    chatDownloading = true,
                    downloadJobId = job.Key.ToString(),
                    chatDownloadJobId = chatDownloadJob.Key.ToString()
                };

                context.Add(dbStream);
                context.SaveChanges();
            }
            
            var schedulerFactory = new StdSchedulerFactory(QuartzSchedulers.RamScheduler());
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.Start();
            
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity(triggerIdentity)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);
            
            PrepareLiveChat(chatDownloadJob, stream.streamId);
            return Task.CompletedTask;
        }
        
        public void PrepareLiveChat(IJobDetail chatDownloadJob, long streamId) {
            var schedulerFactory = new StdSchedulerFactory(QuartzSchedulers.RamScheduler());
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.Start();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("LiveStreamDownloadTrigger" + streamId)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(chatDownloadJob, trigger);
        }
    }
}