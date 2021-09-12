#nullable enable
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Quartz.Impl;
using voddy.Controllers.BackgroundTasks.StreamDownloads;

namespace voddy {
    public class JobHelpers {
        private static Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();
        public static void NormalJob<T>(string jobIdentity, string triggerIdentity, NameValueCollection schedulerSelection) where T : IJob {
            IJobDetail job = JobBuilder.Create<T>()
                .WithIdentity(jobIdentity)
                .Build();
            
            var schedulerFactory = new StdSchedulerFactory(schedulerSelection);
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.Start();
            
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity(triggerIdentity)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
        
        public static Task CancelJob(string name, string? group, NameValueCollection scheulderSelection) {
            var scheulderFactory = new StdSchedulerFactory(scheulderSelection);
            IScheduler scheduler = scheulderFactory.GetScheduler().Result;
            if (!scheduler.IsStarted) {
                return Task.FromException(new Exception("Scheduler not started."));
            }

            if (group != null) {
                if (scheduler.Interrupt(new JobKey(name, group)).Result) {
                    _logger.Info($"Cancelled {name}.{group} job.");
                    return Task.CompletedTask;
                } else {
                    _logger.Error($"Job {name}.{group} not found.");
                    return Task.FromException(new Exception($"Job {name}.{group} not found."));
                };
            } else {
                if (scheduler.Interrupt(new JobKey(name)).Result) {
                    _logger.Info($"Cancelled {name} job.");
                    return Task.CompletedTask;
                } else {
                    _logger.Error($"Job {name} not found.");
                    return Task.FromException(new Exception($"Job {name} not found."));
                };
            }
            
            return Task.CompletedTask;
        }
    }
}