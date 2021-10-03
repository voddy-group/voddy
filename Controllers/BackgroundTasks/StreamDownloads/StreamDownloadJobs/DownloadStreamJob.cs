using System;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.StreamDownloads {
    public class DownloadStreamJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
            handleDownloadStreamsLogic.DownloadStream(
                (StreamExtended)jobDataMap["stream"],
                jobDataMap.GetString("title"),
                jobDataMap.GetString("streamDirectory"),
                jobDataMap.GetString("formatId"),
                jobDataMap.GetString("url"),
                jobDataMap.GetBooleanValue("isLive"),
                jobDataMap.GetLongValue("youtubeDlVideoInfoDuration"),
                context.CancellationToken).Wait();
            return Task.CompletedTask;
        }
    }
}