using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using voddy.Data;

namespace voddy.Controllers.LiveStreams {
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("liveStream")]
    public class CheckLiveStatus : ControllerBase {
        [HttpPost]
        [Route("enableMonitoring")]
        public void EnableLiveStreamMonitoring(int streamerId) {
        }
    }
}