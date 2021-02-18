﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace voddy.Controllers {
    [ApiController]
    [Route("auth/twitchAuth/redirect")]
    public class TwitchAuthRedirect : ControllerBase {
        private readonly ILogger<TwitchAuthRedirect> _logger;

        public TwitchAuthRedirect(ILogger<TwitchAuthRedirect> logger) {
            _logger = logger;
        }
        
        [HttpGet]
        public void Get(string code) {
            HttpContext.Session.SetString("code", code);
        }
    }
}