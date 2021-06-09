using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    [ApiController]
    [Route("auth/twitchAuth")]
    public class PostTwitchAuthController : ControllerBase {
        private readonly ILogger<PostTwitchAuthController> _logger;
        

        public PostTwitchAuthController(ILogger<PostTwitchAuthController> logger) {
            _logger = logger;
        }

        [HttpPost]
        public string Post(TwitchAuthBuilder.UserAuth userAuth) {
            TwitchAuthBuilder twitchAuthBuilder = new TwitchAuthBuilder();
            return twitchAuthBuilder.BuildTwitchAuth(userAuth, HttpContext);
        }
    }
}