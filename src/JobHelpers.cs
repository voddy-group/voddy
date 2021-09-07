using Quartz;
using Quartz.Impl;
using voddy.Controllers.BackgroundTasks.StreamDownloads;

namespace voddy {
    public class JobHelpers {
        public static void NormalJob<T>(string jobIdentity, string triggerIdentity) where T : IJob {
            IJobDetail job = JobBuilder.Create<T>()
                .WithIdentity(jobIdentity)
                .Build();
            
            var schedulerFactory = new StdSchedulerFactory(QuartzSchedulers.PrimaryScheduler());
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.Start().Start();
            
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity(triggerIdentity)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}