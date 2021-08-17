using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    public class TwitchTokenLogic {
        public Tuple<int, Response> GetTwitchAuthTokenLogic(ISession session) {
            string url = $"https://id.twitch.tv/oauth2/token" +
                         $"?client_id={session.GetString("clientId")}" +
                         $"&client_secret={session.GetString("clientSecret")}" +
                         $"&code={session.GetString("code")}" +
                         $"&grant_type=authorization_code" +
                         $"&redirect_uri=https://localhost:5001/auth/twitchAuth/redirect";
            var client = new RestClient(url) {Timeout = -1};
            var request = new RestRequest(Method.POST);
            IRestResponse twitchResponse = client.Execute(request);

            Response response = new Response();

            if (twitchResponse.IsSuccessful) {
                var nuResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(twitchResponse.Content);
                response.success = true;
                response.error = "";
                Authentication authentication = new Authentication();
                authentication.clientId = session.GetString("clientId");
                authentication.clientSecret = session.GetString("clientSecret");
                authentication.accessToken = nuResponse.access_token;
                authentication.refreshToken = nuResponse.refresh_token;
                authentication.service = "twitch";
                SaveAuthToDb(authentication);

                UserDetails userDetails = new UserDetails();
                userDetails.SaveUserDataToDb();
                
                session.Clear();
                return new Tuple<int, Response>(200, response);
            }

            response.success = false;
            response.error = twitchResponse.Content;
            return new Tuple<int, Response>(400, response);
        }
        
        public void SaveAuthToDb(Authentication authentication) {
            using (var context = new MainDataContext()) {
                var auth = context.Authentications.FirstOrDefault(item => item.service == "twitch");

                if (auth != null) {
                    auth.clientId = authentication.clientId;
                    auth.clientSecret = authentication.clientSecret;
                    auth.accessToken = authentication.accessToken;
                    auth.refreshToken = authentication.refreshToken;
                    context.SaveChanges();
                    return;
                }

                context.Authentications.Add(authentication);
                context.SaveChanges();
            }
        }
        
        public new class Response {
            public bool success { get; set; }
            public string error { get; set; }
        }

        public class AccessTokenResponse {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public List<string> scope { get; set; }
            public string token_type { get; set; }
        }
    }
}