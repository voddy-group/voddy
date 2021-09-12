using System.Threading.Tasks;
using Quartz;
using voddy.Controllers.BackgroundTasks.RecurringJobs;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.BackgroundTasks.StreamDownloads.StreamDownloadJobs {
    public class LiveStreamDownloadJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.LiveStreamDownloadJob(
                (Streamer)jobDataMap["streamer"],
                (HandleDownloadStreamsLogic.Data)jobDataMap["stream"]);
            return Task.CompletedTask;
        }
    }
}