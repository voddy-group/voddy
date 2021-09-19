using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class StreamerCheckForUpdatesJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.StreamerCheckForUpdates();
            JobHelpers.SetJobLastRunDateTime(context);
            return Task.CompletedTask;
        }
    }
}