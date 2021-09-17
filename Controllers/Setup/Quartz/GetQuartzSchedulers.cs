using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

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
                    foreach (var trigger in triggers) {
                        var job = new Job {
                            name = trigger.JobKey.Group + "." + trigger.JobKey.Name,
                            lastFireDateTime = trigger.GetPreviousFireTimeUtc().GetValueOrDefault().LocalDateTime
                        };
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
            public DateTimeOffset? lastFireDateTime { get; set; }
        }

        public class Scheduler {
            public string name { get; set; }
            public int status { get; set; }
            public List<Job> jobs { get; set; }
        }
    }


}