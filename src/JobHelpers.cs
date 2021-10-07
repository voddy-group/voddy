#nullable enable
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Quartz.Impl;
using voddy.Controllers.BackgroundTasks.StreamDownloads;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;
using voddy.Exceptions.Quartz;

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
            var schedulerFactory = new StdSchedulerFactory(scheulderSelection);
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;

            if (group != null && scheduler.CheckExists(new JobKey(name, group)).Result) {
                if (scheduler.Interrupt(new JobKey(name, group)).Result) {
                    _logger.Info($"Cancelled {name}.{group} job.");
                    return Task.CompletedTask;
                } else {
                    _logger.Error($"Job {name}.{group} not found.");
                    return Task.FromException(new MissingJobException($"Job {name}.{group} not found."));
                }
            } else if (scheduler.CheckExists(new JobKey(name)).Result) {
                if (scheduler.Interrupt(new JobKey(name)).Result) {
                    _logger.Info($"Cancelled {name} job.");
                    return Task.CompletedTask;
                } else {
                    _logger.Error($"Job {name} not found.");
                    return Task.FromException(new MissingJobException($"Job {name} not found."));
                }
            } else {
                return Task.FromException(new MissingJobException($"Job {name}.{group} not found."));
            }


            return Task.CompletedTask;
        }

        public static Task SetJobLastRunDateTime(IJobExecutionContext context) {
            using (var dbContext = new MainDataContext()) {
                var existingRecord = dbContext.JobTriggerExecutions.FirstOrDefault(item =>
                    item.Name == context.JobDetail.Key.Name && item.Group == context.JobDetail.Key.Group);

                if (existingRecord != null) {
                    existingRecord.LastFireDateTime = DateTime.Now;
                } else {
                    var jobTriggerExecutionRecord = new JobTriggerExecution {
                        Name = context.Trigger.Key.Name,
                        Group = context.Trigger.Key.Group,
                        Scheduler = context.Scheduler.SchedulerName,
                        LastFireDateTime = DateTime.Now
                    };

                    dbContext.Add(jobTriggerExecutionRecord);
                }

                dbContext.SaveChanges();
            }

            return Task.CompletedTask;
        }
    }
}