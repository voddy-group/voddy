using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class RemoveLeftOverLiveStreamDownloadsJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.RemoveLeftOverLiveStreamDownloads();
            return Task.CompletedTask;
        }
    }
}