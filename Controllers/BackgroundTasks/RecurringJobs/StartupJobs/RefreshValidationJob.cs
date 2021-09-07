using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class RefreshValidationJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.RefreshValidation();
            return Task.CompletedTask;
        }
    }
}