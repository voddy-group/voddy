using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using voddy.Databases.Main;

namespace voddy.Controllers.Setup.Status {
    [ApiController]
    [Route("status")]
    public class Status : ControllerBase {
        [HttpGet]
        public StatusReturn GetStatus() {
            StatusLogic statusLogic = new StatusLogic();
            return statusLogic.GetStatusLogic();
        }
    }


}