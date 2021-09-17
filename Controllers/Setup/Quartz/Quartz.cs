using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
    }
}