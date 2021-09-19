using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class RemoveTempJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.RemoveTemp();
            JobHelpers.SetJobLastRunDateTime(context);
            return Task.CompletedTask;
        }
    }
}