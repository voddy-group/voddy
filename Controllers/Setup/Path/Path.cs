using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace voddy.Controllers.Setup.Path {
    [ApiController]
    [Route("path")]
    public class Path : ControllerBase {
        private PathLogic PathLogic { get; }
        public Path() {
            PathLogic = new PathLogic();
        }
        
        [HttpPut]
        public IActionResult UpdatePath([FromBody] UpdatePathJson data) {
            if (PathLogic.UpdatePathLogic(data)) {
                return Ok();
            }

            return NotFound();
        }

        /*[HttpGet]
        [Route("migrationRunning")]
        public Migration CheckIfMigrationRunning() {
            return PathLogic.CheckIfMigrationRunningLogic();
        }*/
        [HttpGet]
        public RootPathJson GetCurrentRootPath() {
            return PathLogic.GetCurrentPathLogic();
        }
    }

    public class RootPathJson {
        public string Path { get; set; }
    }
    public class UpdatePathJson {
        public string NewPath { get; set; }
        //public bool AlreadyMoved { get; set; }
    }
}