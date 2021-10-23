using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using Quartz;
using voddy.Exceptions.Streams;

namespace voddy.Controllers.BackgroundTasks.LiveStreamDownloads.LiveStreamDownloadJobs {
    public class LiveStreamDownloadJob : IJob {
        private Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();

        public Task Execute(IJobExecutionContext context) {
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            LiveStreamDownload liveStreamDownload = new LiveStreamDownload(new DirectoryInfo(jobDataMap.GetString("streamDirectory")));
            liveStreamDownload.GetVodM3U8(jobDataMap.GetString("url"));
            try {
                liveStreamDownload.GetVodParts(context.CancellationToken);
            } catch (TsFileNotFound e) {
                // test if the stream has gone down.
                try {
                    liveStreamDownload.GetVodM3U8(jobDataMap.GetString("url"));
                } catch (Exception exception) {
                    if (!exception.Message.Contains("is offline")) {
                        // stream is offline, must have finished, so is not an error.
                        // stream has not finished, throw
                        _logger.Error($"Error occured while downloading a live stream. {exception.Message}");
                        liveStreamDownload.CleanUpFiles();
                        throw;
                    }
                }
            }

            liveStreamDownload.CombineTsFiles(jobDataMap.GetString("title"), jobDataMap.GetLongValue("streamId"));
            liveStreamDownload.CleanUpFiles();
            // need to rename file as ffmpeg does not work with special characters.
            File.Move($"{liveStreamDownload._rootDirectory.FullName}/stream.mp4", $"{liveStreamDownload._rootDirectory.FullName}/{jobDataMap.GetString("title")}.{jobDataMap.GetLongValue("streamId")}.mp4");
            HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
            handleDownloadStreamsLogic.SetDownloadToFinished(jobDataMap.GetLongValue("streamId"), true);


            return Task.CompletedTask;
        }
    }
}