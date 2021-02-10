using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace voddy.Controllers {
    [ApiController]
    [Route("auth/twitchAuth")]
    public class PostTwitchAuthController : ControllerBase {
        private readonly ILogger<PostTwitchAuthController> _logger;
        private UserAuth _userAuth = new UserAuth(); 

        public PostTwitchAuthController(ILogger<PostTwitchAuthController> logger) {
            _logger = logger;
        }

        [HttpPost]
        public bool Post(UserAuth userAuth) {
            _userAuth.clientId = userAuth.clientId;
            _userAuth.clientSecret = userAuth.clientSecret;
            return true;
        }

        public class UserAuth {
            public string clientId { get; set; }
            public string clientSecret { get; set; }
        }
    }
}