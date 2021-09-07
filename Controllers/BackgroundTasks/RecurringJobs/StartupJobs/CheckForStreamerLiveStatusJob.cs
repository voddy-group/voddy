using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class CheckForStreamerLiveStatusJob : IJob {

        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobs = new StartupJobsLogic();
            startupJobs.CheckForStreamerLiveStatus();
            return Task.CompletedTask;
        }
    }
}