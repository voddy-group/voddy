using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class CheckStreamFileExistsJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.CheckStreamFileExists();
            JobHelpers.SetJobLastRunDateTime(context);
            return Task.CompletedTask;
        }
    }
}