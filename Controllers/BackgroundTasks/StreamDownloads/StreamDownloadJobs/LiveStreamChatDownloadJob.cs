using System.Threading.Tasks;
using Quartz;
using voddy.Controllers.LiveStreams;

namespace voddy.Controllers.BackgroundTasks.StreamDownloads.StreamDownloadJobs {
    public class LiveStreamChatDownloadJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            LiveStreamChatLogic liveStreamChatLogic = new LiveStreamChatLogic();
            return liveStreamChatLogic.DownloadLiveStreamChatLogic(
                jobDataMap.GetString("channel"),
                jobDataMap.GetLongValue("streamId"),
                context.CancellationToken);
            ;
        }
    }
}