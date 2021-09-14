using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace voddy.Controllers.Setup.Quartz {
    [ApiController]
    [Route("quartz")]
    public class Quartz : ControllerBase {
        [HttpGet]
        public QuartzLogic.QuartzApiResponse GetQuartzSchedulers() {
            QuartzLogic quartzLogic = new QuartzLogic();
            return quartzLogic.GetQuartzSchedulersLogic();
        }
    }
}