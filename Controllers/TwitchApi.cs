using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("twitchApi")]
    public class TwitchApi : ControllerBase {
        private readonly ILogger<TwitchApi> _logger;
        private TwitchApiHelpers _twitchApiHelpers = new TwitchApiHelpers();

        public TwitchApi(ILogger<TwitchApi> logger) {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] ApiRequest body) {
            var response = _twitchApiHelpers.TwitchRequest(body.url, Method.GET);

            if (response.IsSuccessful) {
                return Ok();
            }

            return Unauthorized();
        }

        [HttpGet]
        [Route("stream/search")]
        public SearchResult Search(string term) {
            var response =
                _twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/search/channels" +
                                               $"?query={term}" +
                                               "&first=15",
                    Method.GET);

            var deserializeResponse = JsonConvert.DeserializeObject<SearchResult>(response.Content);
            return deserializeResponse;
        }

        public class ApiRequest {
            public string url { get; set; }
        }

        public class SearchResultData {
            public string broadcaster_language { get; set; }
            public string broadcaster_login { get; set; }
            public string display_name { get; set; }
            public string game_id { get; set; }
            public string id { get; set; }
            public bool is_live { get; set; }
            public IList<string> tags_ids { get; set; }
            public string thumbnail_url { get; set; }
            public string title { get; set; }
            public string started_at { get; set; }

        }
        public class SearchResult {
            public IList<SearchResultData> data { get; set; }
        }
    }
}