using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.StreamDownloads {
    public class GenerateVideoThumbnailJob : IJob{
        public Task Execute(IJobExecutionContext context) {
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
            handleDownloadStreamsLogic.GenerateVideoThumbnail(
                jobDataMap.GetLongValue("streamId"),
                jobDataMap.GetString("streamFile"));
            return Task.CompletedTask;
        }
    }
}