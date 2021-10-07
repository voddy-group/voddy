using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace voddy.Controllers.Setup.Quartz {
    public class QuartzDeleteJob {
        public Task QuartzDeleteJobLogic(QuartzExecuteJob.QuartzExecuteJobRequest requestBody) {
            MethodInfo methodInfo = typeof(QuartzSchedulers).GetMethod(requestBody.scheduler);
            if (methodInfo is not null) {
                var schedulerFactory = new StdSchedulerFactory();
                IScheduler scheduler = schedulerFactory.GetScheduler().Result;

                var splitJobName = requestBody.name.Split(".");
                return JobHelpers.CancelJob(splitJobName[1], splitJobName[0], (NameValueCollection)methodInfo.Invoke(this, null));
            } else {
                return Task.FromException(new SchedulerException("Scheduler does not exist."));
            }
        }
    }
}