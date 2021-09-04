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
                jobDataMap.GetString("youtubeDlVideoInfoUrl"),
                jobDataMap.GetBooleanValue("isLive"),
                jobDataMap.GetLongValue("youtubeDlVideoInfoDuration")).Wait();
            return Task.CompletedTask;
        }
    }
}