using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using voddy.Data;
using voddy.Models;
using HttpResponse = Microsoft.AspNetCore.Http.HttpResponse;

namespace voddy.Controllers {
    [ApiController]
    [Route("auth/twitchAuth/token")]
    public class GetTwitchToken: ControllerBase {
        private readonly ILogger<GetTwitchToken> _logger;
        private DataContext _dataContext;
        
        public GetTwitchToken(ILogger<GetTwitchToken> logger, DataContext sc) {
            _logger = logger;
            _dataContext = sc;
        }

        [HttpGet]
        public string Get() {
            string url = $"https://id.twitch.tv/oauth2/token" +
                         $"?client_id={HttpContext.Session.GetString("clientId")}" +
                         $"&client_secret={HttpContext.Session.GetString("clientSecret")}" +
                         $"&code={HttpContext.Session.GetString("code")}" +
                         $"&grant_type=authorization_code" +
                         $"&redirect_url=https://localhost:5001/auth/twitchAuth/redirect";
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
                authentication.accessToken = HttpContext.Session.GetString(nuResponse.access_token);
                authentication.refreshToken = HttpContext.Session.GetString(nuResponse.refresh_token);
                _dataContext.Authentications.Add(authentication);
                _dataContext.SaveChanges();
                Console.WriteLine(authentication.accessToken);
                Console.WriteLine(authentication.clientId);
                Console.WriteLine(authentication.clientSecret);
                Console.WriteLine(authentication.refreshToken);
            } else {
                
                response.success = false;
                response.error = twitchResponse.ErrorMessage;
            }

            return "HttpResponseMessage";
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