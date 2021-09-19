using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.Quartz {
    public class GetQuartzSchedulers {
        public QuartzApiResponse GetQuartzSchedulersLogic() {
            QuartzApiResponse quartzApiResponse = new QuartzApiResponse();
            quartzApiResponse.schedulers = new List<Scheduler>();

            var schedulerFactory = new StdSchedulerFactory();
            var schedulers = schedulerFactory.GetAllSchedulers().Result;
            foreach (var scheduler in schedulers) {
                int status;
                if (scheduler.IsStarted) {
                    status = 1; // started
                } else if (scheduler.InStandbyMode) {
                    status = 2; // standby
                } else {
                    status = 3; // shutdown/inactive/dead
                }

                quartzApiResponse.schedulers.Add(new Scheduler {
                    name = scheduler.SchedulerName,
                    status = status,
                    jobs = GetSchedulerJobs(scheduler)
                });
            }

            return quartzApiResponse;
        }

        private List<Job> GetSchedulerJobs(IScheduler scheduler) {
            List<Job> jobsList = new List<Job>();
            foreach (var groupName in scheduler.GetJobGroupNames().Result) {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(groupName);
                foreach (var jobKey in scheduler.GetJobKeys(groupMatcher).Result) {
                    var triggers = scheduler.GetTriggersOfJob(jobKey).Result;
                    List<JobTriggerExecution> jobTriggerExecutionsList;
                    using (var dbContext = new MainDataContext()) {
                        jobTriggerExecutionsList = dbContext.JobTriggerExecutions.ToList();
                    }
                    foreach (var trigger in triggers) {
                        var triggerLastExecution = jobTriggerExecutionsList.FirstOrDefault(item =>
                            item.Name == trigger.JobKey.Name && item.Group == trigger.JobKey.Group);
                        var job = new Job {
                            name = trigger.JobKey.Group + "." + trigger.JobKey.Name,
                        };
                        if (triggerLastExecution != null) {
                            job.lastFireDateTime = triggerLastExecution.LastFireDateTime;
                        }
                        if (trigger is ICronTrigger cronTrigger) {
                            job.cron = cronTrigger.CronExpressionString;
                            var nextFireDateTime = cronTrigger.GetFireTimeAfter(DateTimeOffset.Now);
                            if (nextFireDateTime != null) {
                                job.nextFire = (nextFireDateTime.Value - DateTimeOffset.Now);
                            }
                        }
                        jobsList.Add(job);
                    }
                }
            }

            return jobsList;
        }
        
        public class QuartzApiResponse {
            public List<Scheduler> schedulers { get; set; }
        }

        public class Job {
            public string name { get; set; }
            public string cron { get; set; }
            public TimeSpan nextFire { get; set; }
            public DateTime? lastFireDateTime { get; set; }
        }

        public class Scheduler {
            public string name { get; set; }
            public int status { get; set; }
            public List<Job> jobs { get; set; }
        }
    }


}