using Microsoft.AspNetCore.Mvc;

namespace voddy.Controllers.Setup.Update {
    [ApiController]
    [Route("update")]
    public class Update : ControllerBase {
        [HttpGet]
        [Route("check")]
        public UpdateCheckReturn CheckUpdates() {
            UpdateLogic updateLogic = new UpdateLogic();

            UpdateCheckReturn updateCheckReturn = new UpdateCheckReturn {
                updateAvailable = updateLogic.CheckForUpdates(),
                currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };

            return updateCheckReturn;
        }
    }

    public class UpdateCheckReturn {
        public bool updateAvailable { get; set; }
        public string currentVersion { get; set; }
    }
}