using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class TrimLogsJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            startupJobsLogic.TrimLogs();
            return Task.CompletedTask;
        }
    }
}