using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Setup.TwitchAuthentication;
using voddy.Controllers.Structures;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
    public class TwitchApiLogic {
        private TwitchApiHelpers _twitchApiHelpers = new TwitchApiHelpers();

        public bool PostLogic(ApiRequest body) {
            var response = _twitchApiHelpers.TwitchRequest(body.url, Method.GET);
            
                        if (response.IsSuccessful) {
                            return true;
                        }
            
                        return false;
        }
        
        public List<StreamerReturn> SearchLogic(string term) {
            var response =
                _twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/search/channels" +
                                                $"?query={term}" +
                                                "&first=15",
                    Method.GET);

            var deserializedResponse = JsonConvert.DeserializeObject<SearchResult>(response.Content);
            
            List<Streamer> addedStreamers;
            using (var context = new DataContext()) {
                addedStreamers = context.Streamers.ToList();
            }
            string listOfIds = "?id=";
            
            for (int i = 0; i < deserializedResponse.data.Count; i++) {
                if (i != deserializedResponse.data.Count - 1) {
                    listOfIds += deserializedResponse.data[i].id + "&id=";
                } else {
                    listOfIds += deserializedResponse.data[i].id;
                }
            }

            var userResponse =
                _twitchApiHelpers.TwitchRequest($"https://api.twitch.tv/helix/users{listOfIds}", Method.GET);
            
            var deserializedUserResponse = JsonConvert.DeserializeObject<UserJsonClass.User>(userResponse.Content);


            return StreamerListBuilder(addedStreamers, deserializedUserResponse);
        }
        
        public List<StreamerReturn> StreamerListBuilder(List<Streamer> streamerList, UserJsonClass.User userList) {
            List<StreamerReturn> returnList = new List<StreamerReturn>();

            for (int i = 0; i < userList.data.Count; i++) {
                var streamer = new StreamerReturn {
                    displayName = userList.data[i].display_name,
                    streamerId = userList.data[i].id,
                    description = userList.data[i].description,
                    thumbnailLocation = userList.data[i].profile_image_url,
                    username = userList.data[i].login,
                    viewCount = userList.data[i].view_count
                };
                for (var x = 0; x < streamerList.Count; x++) {
                    if (streamerList[x].streamerId == userList.data[i].id) {
                        streamer.alreadyAdded = true;
                        streamer.id = streamerList[x].id;
                    }
                }

                returnList.Add(streamer);
            }

            return returnList;
        }

        public List<StreamerReturn> GetFollowedChannelUsersLogic() {
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
            
            List<Streamer> addedStreamers;
            using (var context = new DataContext()) {
                addedStreamers = context.Streamers.ToList();
            }

            return StreamerListBuilder(addedStreamers, deserializedResponse);
        }
        
        public FollowListJsonClass.FollowList GetFollowedChannels() {
            string userId;
            using (var context = new DataContext()) {
                while (true) {
                    var data = context.Configs.FirstOrDefault(item => item.key == "userId");
                    if (data != null) {
                        userId = data.value;
                        break;
                    }

                    UserDetails userDetails = new UserDetails();
                    userDetails.SaveUserDataToDb();
                }
            }

            var response =
                _twitchApiHelpers.TwitchRequest($"https://api.twitch.tv/helix/users/follows?from_id={userId}&first=15",
                    Method.GET);

            return JsonConvert.DeserializeObject<FollowListJsonClass.FollowList>(response.Content);
        }
        
        public class ApiRequest {
            public string url { get; set; }
        }

        public class SearchResultData {
            public string broadcaster_language { get; set; }
            public string broadcaster_login { get; set; }
            public string display_name { get; set; }
            public bool alreadyAdded { get; set; }
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
    
        public class StreamerReturn: Streamer {
            public bool alreadyAdded { get; set; }
        }
    }
}