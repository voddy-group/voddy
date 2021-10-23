using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quartz;

namespace voddy {
    public class JobFailureHandler : IJobListener {
        public string Name => "JobFailureHandler";
        private Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();
        private int maxRetries = 3;

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken) {
            if (context.JobDetail.JobDataMap.Contains("retry") && context.JobDetail.JobDataMap.GetBooleanValue("retry")) {
                if (!context.JobDetail.JobDataMap.Contains("retryCount")) {
                    context.JobDetail.JobDataMap.Put("retryCount", 1);
                } else {
                    int numberOfTries = context.JobDetail.JobDataMap.GetIntValue("retryCount");
                    context.JobDetail.JobDataMap.Put("retryCount", numberOfTries + 1);
                }
            }

            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException exception, CancellationToken cancellationToken) {
            if (exception == null) {
                return Task.CompletedTask;
            }

            if (context.JobDetail.JobDataMap.Contains("retry")) {
                int numberOfTries = context.JobDetail.JobDataMap.GetIntValue("retryCount");

                if (numberOfTries > maxRetries) {
                    _logger.Error($"{context.JobDetail.Key} has thrown too many errors, the job will be cancelled.");
                    return Task.CompletedTask;
                }

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(context.Trigger.Key + "Retry" + numberOfTries)
                    .StartAt(DateTime.Now.AddHours(numberOfTries)) // change
                    .Build();

                _logger.Warn($"{context.JobDetail.Key} has thrown {exception}. Will retry again in {numberOfTries} hour(s).");

                context.Scheduler.RescheduleJob(context.Trigger.Key, trigger).Wait();
            }

            return Task.CompletedTask;
        }
    }
}