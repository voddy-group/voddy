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
    [Route("twitchApi")]
    public class TwitchApi : ControllerBase {
        private readonly ILogger<TwitchApi> _logger;

        public TwitchApi(ILogger<TwitchApi> logger) {
            _logger = logger;
        }

        [HttpPost]
        public void Post([FromBody] ApiRequest body) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = JsonConvert.DeserializeObject<GetStreams>(twitchApiHelpers.TwitchRequest(body.url, Method.GET).Content);
            
            Console.WriteLine(response.data[0].user_name);
        }

        public class ApiRequest {
            public string url { get; set; }
        }

        public class Data {
            public string id { get; set; }
            public string user_id { get; set; }
            public string user_login { get; set; }
            public string user_name { get; set; }
            public string game_id { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public int viewer_count { get; set; }
            public DateTime started_at { get; set; }
            public string language { get; set; }
            public string thumbnail_url { get; set; }

        }
        public class Pagination {
            public string cursor { get; set; }

        }
        public class GetStreams {
            public IList<Data> data { get; set; }
            public Pagination pagination { get; set; } 

        }
    }
}