using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using voddy.Controllers.JSONClasses;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    [ApiController]
    [Route("ytDlp")]
    public class YtDlpTest : ControllerBase {
        private readonly ILogger<YtDlpTest> _logger;

        public YtDlpTest(ILogger<YtDlpTest> logger) {
            _logger = logger;
        }

        [HttpGet]
        [Route("test")]
        public IActionResult TestYtDlp(string path) {
            YtDlpTestLogic ytDlpTestLogic = new YtDlpTestLogic();
            try {
                ytDlpTestLogic.TestYtDlpLogic(path);
            } catch (Exception e) {
                return StatusCode(500, new ApiError.Error { error = e.Message });
            }

            return Ok();
        }

        [HttpGet]
        [Route("update")]
        public IActionResult UpdateYtDlp() {
            YtDlpTestLogic ytDlpTestLogic = new YtDlpTestLogic();
            bool success = ytDlpTestLogic.UpdateYtDlpLogic();
            if (success) {
                return Ok();
            }

            return NotFound();
        }
    }
}