using System.Threading.Tasks;
using Quartz;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class CleanUpDatabaseJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            CleanUpDatabase cleanUpDatabase = new CleanUpDatabase();
            cleanUpDatabase.TrimLogs();
            JobHelpers.SetJobLastRunDateTime(context);
            return Task.CompletedTask;
        }
    }
}