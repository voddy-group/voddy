using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using RestSharp;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy {
    public class TwitchApiHelpers {
        //private DataContext _dataContext;
        private Authentication _authentication;
        private string url { get; set; }
        private Method method { get; set; }

        public TwitchApiHelpers() {
            using (var context = new MainDataContext()) {
                _authentication = context.Authentications.FirstOrDefault(item => item.service == "twitch");
            }
        }


        public IRestResponse TwitchRequest(string url, Method method) {
            this.url = url;
            this.method = method;

            return ValidTokenCheck(url, method);
        }

        public IRestResponse LegacyTwitchRequest(string url, Method method) {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddHeader("Client-Id", _authentication.clientId);
            request.AddHeader("Accept", "application/vnd.twitchtv.v5+json; charset=UTF-8");
            return client.Execute(request);
        }

        private IRestResponse ValidTokenCheck(string url, Method method) {
            IRestResponse response = Request(url, method);

            if (response.IsSuccessful) {
                Console.WriteLine(url);
                return response;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized) { // access token expired
                var client = new RestClient("https://id.twitch.tv/oauth2/token" +
                    "?grant_type=refresh_token" +
                    $"&refresh_token={_authentication.refreshToken}" +
                    $"&client_id={_authentication.clientId}" +
                    $"&client_secret={_authentication.clientSecret}");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                IRestResponse reAuth = client.Execute(request); // generate a new one

                if (reAuth.IsSuccessful) {
                    var reAuthResponse = JsonConvert.DeserializeObject<RefreshToken>(reAuth.Content);
                    
                    using (var context = new MainDataContext()) {
                        _authentication = new Authentication();
                        _authentication = context.Authentications.FirstOrDefault(item => item.service == "twitch");

                        _authentication.accessToken = reAuthResponse.access_token;
                        _authentication.refreshToken = reAuthResponse.refresh_token;

                        context.SaveChanges(); // save new credentials to db
                    }

                    Console.WriteLine(url);
                    return Request(url, method); // run original request again using new credentials
                } else {
                    return response;
                    // refresh token not working; NEED TO HANDLE THIS. Likely user will have to re-auth.
                }
                
            }

            return response;
        }

        private IRestResponse Request(string url, Method method) {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddHeader("client-id", _authentication.clientId);
            request.AddHeader("Authorization", $"Bearer {_authentication.accessToken}");
            return client.Execute(request);
        }

        public class RefreshToken {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }
    }
}