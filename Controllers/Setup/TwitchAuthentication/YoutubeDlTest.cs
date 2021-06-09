using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    [ApiController]
    [Route("youtubeDl")]
    public class YoutubeDlTest : ControllerBase {
        private readonly ILogger<YoutubeDlTest> _logger;

        public YoutubeDlTest(ILogger<YoutubeDlTest> logger) {
            _logger = logger;
        }

        [HttpGet]
        [Route("test")]
        public YoutubeDlTestLogic.TestResponse TestYoutubeDl(string path) {
            YoutubeDlTestLogic youtubeDlTestLogic = new YoutubeDlTestLogic();
            return youtubeDlTestLogic.TestYoutubeDlLogic(path);
        }

        [HttpGet]
        [Route("update")]
        public IActionResult UpdateYoutubeDl() {
            YoutubeDlTestLogic youtubeDlTestLogic = new YoutubeDlTestLogic();
            bool success = youtubeDlTestLogic.UpdateYoutubeDlLogic();
            if (success) {
                return Ok();
            }

            return NotFound();
        }
    }
}