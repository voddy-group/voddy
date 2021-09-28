using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    public class UserDetails {
        public string SaveUserDataToDb() {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/users", Method.GET);

            var deserializedResponse = JsonConvert.DeserializeObject<UserJsonClass.User>(response.Content);

            string currentUserId = GlobalConfig.GetGlobalConfig("userId");
            if (currentUserId == null) {
                GlobalConfig.SetGlobalConfig("userId", deserializedResponse.data[0].id.ToString());
            }

            string currentUserName = GlobalConfig.GetGlobalConfig("userName");
            if (currentUserName == null) {
                GlobalConfig.SetGlobalConfig("userName", deserializedResponse.data[0].login);
            }

            return currentUserName;
        }
    }
}