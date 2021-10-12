using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.StreamDownloads.StreamDownloadJobs {
    [PersistJobDataAfterExecution]
    public class ChatDownloadJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            handleDownloadStreamsLogic.DownloadChat(jobDataMap.GetLongValue("streamId")).Wait();
            return Task.CompletedTask;
        }
    }
}