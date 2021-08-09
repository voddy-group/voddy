using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup {
    [ApiController]
    [Route("setup")]
    public class Setup : ControllerBase {
        private SetupLogic _setupLogic { get; set; }
        public Setup() {
            _setupLogic = new SetupLogic();
        }
        
        [HttpGet]
        [Route("globalSettings")]
        public List<Config> GetGlobalSettings() {
            return _setupLogic.GetGlobalSettingsLogic();
        }

        [HttpPut]
        [Route("globalSettings")]
        public IActionResult SetGlobalSettings([FromBody] SetupLogic.GlobalSettings globalSettings) {
            _setupLogic.SetGlobalSettingsLogic(globalSettings);
            return Ok();
        }
    }
}