using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Structures;

namespace voddy.Controllers {
    [ApiController]
    [Route("twitchApi")]
    public class TwitchApi : ControllerBase {
        private readonly ILogger<TwitchApi> _logger;
        readonly TwitchApiLogic twitchApiLogic = new TwitchApiLogic();

        public TwitchApi(ILogger<TwitchApi> logger) {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] TwitchApiLogic.ApiRequest body) {
            if (twitchApiLogic.PostLogic(body)) {
                return Ok();
            }

            return Unauthorized();
        }

        [HttpGet]
        [Route("stream/search")]
        public List<TwitchApiLogic.StreamerReturn> Search(string term) {
            return twitchApiLogic.SearchLogic(term);
        }

        [HttpGet]
        [Route("followed")]
        public List<TwitchApiLogic.StreamerReturn> GetFollowedChannelUsers() {
            return twitchApiLogic.GetFollowedChannelUsersLogic();
        }
    }
}