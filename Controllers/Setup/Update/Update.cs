using Microsoft.AspNetCore.Mvc;

namespace voddy.Controllers.Setup.Update {
    [ApiController]
    [Route("update")]
    public class Update : ControllerBase {
        public UpdateLogic _updateLogic { get; set; }
        public Update() {
            _updateLogic = new UpdateLogic();
        }
        [HttpGet]
        [Route("check")]
        public UpdateCheckReturn CheckUpdates() {
            return _updateLogic.CheckForUpdates();
        }

        [HttpGet]
        [Route("internalCheck")]
        public IActionResult GetUpdates() {
            return Ok(new {
                updateAvailable = _updateLogic.GetUpdatesLogic()
            });
        }
    }

    public class UpdateCheckReturn {
        public bool updateAvailable { get; set; }
        public string latestVersion { get; set; }
    }
}