using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    [ApiController]
    [Route("auth/twitchAuth/redirect")]
    public class TwitchAuthRedirect : ControllerBase {
        private readonly ILogger<TwitchAuthRedirect> _logger;

        public TwitchAuthRedirect(ILogger<TwitchAuthRedirect> logger) {
            _logger = logger;
        }
        
        [HttpGet]
        public void AuthRedirect(string code) {
            HttpContext.Session.SetString("code", code);
        }
    }
}