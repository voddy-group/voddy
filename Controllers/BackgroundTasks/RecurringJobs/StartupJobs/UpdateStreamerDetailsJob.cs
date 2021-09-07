using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Quartz;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs {
    public class UpdateStreamerDetailsJob : IJob {
        public Task Execute(IJobExecutionContext context) {
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            StartupJobsLogic startupJobsLogic = new StartupJobsLogic();
            List<Streamer> streamerList = (List<Streamer>)jobDataMap["listOfStreamers"];
            startupJobsLogic.UpdateStreamerDetails(streamerList);
            return Task.CompletedTask;
        }
    }
}