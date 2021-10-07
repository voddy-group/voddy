using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using voddy.Exceptions.Quartz;

namespace voddy.Controllers.Setup.Quartz {
    [ApiController]
    [Route("quartz")]
    public class Quartz : ControllerBase {
        [HttpGet]
        [Route("schedulers")]
        public GetQuartzSchedulers.QuartzApiResponse GetQuartzSchedulers() {
            GetQuartzSchedulers getQuartzSchedulers = new GetQuartzSchedulers();
            return getQuartzSchedulers.GetQuartzSchedulersLogic();
        }

        [HttpPost]
        [Route("executeJob")]
        public Task ExecuteJob([FromBody] QuartzExecuteJob.QuartzExecuteJobRequest requestBody) {
            QuartzExecuteJob quartzExecuteJob = new QuartzExecuteJob();
            return quartzExecuteJob.QuartzExecuteJobLogic(requestBody);
        }

        [HttpDelete]
        [Route("cancelJob")]
        public IActionResult DeleteJob(string name, string scheduler) {
            QuartzDeleteJob quartzDeleteJob = new QuartzDeleteJob();
            var task = quartzDeleteJob.QuartzDeleteJobLogic(new QuartzExecuteJob.QuartzExecuteJobRequest {
                name = name,
                scheduler = scheduler
            });

            try {
                task.Wait();
            } catch (AggregateException e) {
                return NotFound();
            }

            return Ok();
        }
    }
}