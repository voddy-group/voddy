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

        [HttpGet]
        [Route("followed")]
        public SearchResult GetFollowedChannelUsers() {
            // returns more details about the user, like thumbnails etc.
            var followedUsers = GetFollowedChannels();

            List<string> idList = new List<string>();
            followedUsers.data.ForEach(item => idList.Add(item.to_id));
            string idListString = "?id=";
            idListString += string.Join("&id=", idList);

            var response = _twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/users" + idListString, Method.GET);

            var deserializedResponse = JsonConvert.DeserializeObject<UserJsonClass.User>(response.Content);

            SearchResult result = new SearchResult();
            result.data = new Collection<SearchResultData>();

            for (int i = 0; i < deserializedResponse.data.Count; i++) {
                result.data.Add(new SearchResultData {
                    display_name = deserializedResponse.data[i].display_name,
                    id = deserializedResponse.data[i].id,
                    thumbnail_url = deserializedResponse.data[i].profile_image_url,
                    broadcaster_login = deserializedResponse.data[i].login
                });
            }
            
            return result;
        }

        public FollowListJsonClass.FollowList GetFollowedChannels() {
            string userId = GetUser();

            var response =
                _twitchApiHelpers.TwitchRequest($"https://api.twitch.tv/helix/users/follows?from_id={userId}&first=15",
                    Method.GET);

            return JsonConvert.DeserializeObject<FollowListJsonClass.FollowList>(response.Content);
        }

        public string GetUser() {
            using (var context = new DataContext()) {
                var userId = context.Configs.FirstOrDefault(item => item.key == "userId");

                if (userId != null) {
                    return userId.value;
                }

                var response = _twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/users", Method.GET);

                var deserializedResponse = JsonConvert.DeserializeObject<UserJsonClass.User>(response.Content);

                var newUserId = new Config {
                    key = "userId",
                    value = deserializedResponse.data[0].id
                };

                context.Add(newUserId);

                context.SaveChanges();
                return deserializedResponse.data[0].id;
            }
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