using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.InternalVariables {
    [ApiController]
    [Route("internal")]
    public class InternalVariables : ControllerBase {
        [HttpGet]
        [Route("variables")]
        public List<Config> GetVariables() {
            InternalVariablesLogic internalVariablesLogic = new InternalVariablesLogic();
            return internalVariablesLogic.GetVariablesLogic();
        }
    }
}