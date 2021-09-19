using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class CheckForUpdatesJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.CheckForUpdates();
            JobHelpers.SetJobLastRunDateTime(context);
            return Task.CompletedTask;
        }
    }
}