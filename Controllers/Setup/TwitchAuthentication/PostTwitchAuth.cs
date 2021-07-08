using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    [ApiController]
    [Route("auth")]
    public class PostTwitchAuthController : ControllerBase {
        private readonly ILogger<PostTwitchAuthController> _logger;
        

        public PostTwitchAuthController(ILogger<PostTwitchAuthController> logger) {
            _logger = logger;
        }

        [HttpPost]
        [Route("twitchAuth")]
        public string Post(TwitchAuthBuilder.UserAuth userAuth) {
            TwitchAuthBuilder twitchAuthBuilder = new TwitchAuthBuilder();
            return twitchAuthBuilder.BuildTwitchAuth(userAuth, HttpContext);
        }

        [HttpGet]
        [Route("credentials")]
        public IActionResult GetCredentials() {
            TwitchAuthLogic twitchAuthLogic = new TwitchAuthLogic();
            CredentialsReturn credentialsReturn = twitchAuthLogic.GetCredentialsLogic();
            if (credentialsReturn.clientId != null && credentialsReturn.clientSecret != null) {
                return Ok(credentialsReturn);
            } else {
                return NotFound();
            }
        }
    }
}