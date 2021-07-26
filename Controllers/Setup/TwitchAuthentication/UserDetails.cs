using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    public class UserDetails {
        
        public void SaveUserDataToDb() {
            using (var context = new MainDataContext()) {
                var userId = context.Configs.FirstOrDefault(item => item.key == "userId");

                if (userId != null) {
                    return;
                }

                TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
                
                var response = twitchApiHelpers.TwitchRequest("https://api.twitch.tv/helix/users", Method.GET);

                var deserializedResponse = JsonConvert.DeserializeObject<UserJsonClass.User>(response.Content);

                var newUserId = new Config {
                    key = "userId",
                    value = deserializedResponse.data[0].id.ToString()
                };

                var newUserName = new Config {
                    key = "userName",
                    value = deserializedResponse.data[0].login
                };

                context.Add(newUserId);
                context.Add(newUserName);

                context.SaveChanges();
            }
        }
    }
}