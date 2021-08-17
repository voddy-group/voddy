using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    [ApiController]
    [Route("auth/twitchAuth/token")]
    public class GetTwitchToken : ControllerBase {
        private readonly ILogger<GetTwitchToken> _logger;

        public GetTwitchToken(ILogger<GetTwitchToken> logger) {
            _logger = logger;
        }

        [HttpGet]
        public ObjectResult GetTwitchAuthToken() {
            TwitchTokenLogic twitchTokenLogic = new TwitchTokenLogic();
            Tuple<int, TwitchTokenLogic.Response> returnValue = twitchTokenLogic.GetTwitchAuthTokenLogic(HttpContext.Session);
            return StatusCode(returnValue.Item1, returnValue.Item2);
        }
    }
}