using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        public string Post(string code) {
            Console.WriteLine(code);
            return code;
        }

        [HttpGet]
        public string Get(string code) {
            Console.WriteLine(code);
            return code;
        }
    }
}