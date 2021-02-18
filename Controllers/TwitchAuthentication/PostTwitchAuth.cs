using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using RestSharp;
using voddy.Data;
using voddy.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace voddy.Controllers {
    [ApiController]
    [Route("auth/twitchAuth")]
    public class PostTwitchAuthController : ControllerBase {
        private readonly ILogger<PostTwitchAuthController> _logger;
        private UserAuth _userAuth { get; set; } = new();

        public PostTwitchAuthController(ILogger<PostTwitchAuthController> logger) {
            _logger = logger;
        }

        [HttpPost]
        public string Post(UserAuth userAuth) {
            _userAuth.clientId = userAuth.clientId;
            _userAuth.clientSecret = userAuth.clientSecret;
            List<string> scopes = new List<string> { "viewing_activity_read" };
            ReturnAuthUrl returnAuthUrl = new ReturnAuthUrl();
            returnAuthUrl.url = buildGetAuthorize(userAuth.clientId, userAuth.clientSecret, scopes);
            return JsonSerializer.Serialize(returnAuthUrl); // returns the authorize url
        }



        public string buildGetAuthorize(string clientId, string clientSecret, List<string> scopes) {
            string scope = String.Join(" ", scopes);
            
            HttpContext.Session.SetString("clientId", clientId);
            HttpContext.Session.SetString("clientSecret", clientSecret);

            return $"https://id.twitch.tv/oauth2/authorize" +
                   $"?client_id={clientId}" +
                   $"&redirect_uri=https://localhost:5001/auth/twitchAuth/redirect" +
                   $"&response_type=code" +
                   $"&scope={scope}";
        }

        public class UserAuth {
            public string clientId { get; set; }
            public string clientSecret { get; set; }
        }

        public class ReturnAuthUrl {
            public string url { get; set; }
        }
        

    }
}