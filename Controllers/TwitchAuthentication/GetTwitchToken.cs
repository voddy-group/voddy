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
    [Route("auth/twitchAuth/token")]
    public class GetTwitchToken : ControllerBase {
        private readonly ILogger<GetTwitchToken> _logger;

        public GetTwitchToken(ILogger<GetTwitchToken> logger) {
            _logger = logger;
        }

        [HttpGet]
        public ObjectResult Get() {
            string url = $"https://id.twitch.tv/oauth2/token" +
                         $"?client_id={HttpContext.Session.GetString("clientId")}" +
                         $"&client_secret={HttpContext.Session.GetString("clientSecret")}" +
                         $"&code={HttpContext.Session.GetString("code")}" +
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
                authentication.clientId = HttpContext.Session.GetString("clientId");
                authentication.clientSecret = HttpContext.Session.GetString("clientSecret");
                authentication.accessToken = nuResponse.access_token;
                authentication.refreshToken = nuResponse.refresh_token;
                authentication.service = "twitch";
                SaveAuthToDb(authentication);

                UserDetails userDetails = new UserDetails();
                userDetails.SaveUserDataToDb();
                
                HttpContext.Session.Clear();
                return StatusCode(200, response);
            }

            response.success = false;
            response.error = twitchResponse.Content;
            return StatusCode(400, response);
        }

        public void SaveAuthToDb(Authentication authentication) {
            using (var context = new DataContext()) {
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