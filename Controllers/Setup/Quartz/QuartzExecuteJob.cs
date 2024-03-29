using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace voddy.Controllers.Setup.Quartz {
    public class QuartzExecuteJob {
        public Task QuartzExecuteJobLogic(QuartzExecuteJobRequest requestBody) {
            MethodInfo methodInfo = typeof(QuartzSchedulers).GetMethod(requestBody.scheduler);
            if (methodInfo is not null) {
                var schedulerFactory = new StdSchedulerFactory((NameValueCollection)methodInfo.Invoke(this, null));
                IScheduler scheduler = schedulerFactory.GetScheduler().Result;

                var splitJobName = requestBody.name.Split(".");
                JobKey key = new JobKey(splitJobName[1], splitJobName[0]);

                scheduler.TriggerJob(key);

                if (splitJobName[0] != "Startup") {
                    // if single fire job, remove it after manually executing
                    scheduler.DeleteJob(key);
                }

                return Task.CompletedTask;
            } else {
                return Task.FromException(new SchedulerException("Scheduler does not exist."));
            }
        }

        public class QuartzExecuteJobRequest {
            public string name { get; set; }
            public string scheduler { get; set; }
        }
    }
}