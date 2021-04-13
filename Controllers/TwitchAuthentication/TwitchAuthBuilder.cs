using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace voddy.Controllers {
    public class TwitchAuthBuilder {
        private UserAuth _userAuth { get; set; } = new();

        public string BuildTwitchAuth(UserAuth userAuth, HttpContext httpContext) {
            _userAuth.clientId = userAuth.clientId;
            _userAuth.clientSecret = userAuth.clientSecret;
            List<string> scopes = new List<string> {"viewing_activity_read", "chat:read", "chat:edit"};
            ReturnAuthUrl returnAuthUrl = new ReturnAuthUrl();
            returnAuthUrl.url = buildGetAuthorize(userAuth.clientId, userAuth.clientSecret, scopes, httpContext);
            return JsonSerializer.Serialize(returnAuthUrl); // returns the authorize url
        }

        private string buildGetAuthorize(string clientId, string clientSecret, List<string> scopes,
            HttpContext httpContext) {
            string scope = String.Join(" ", scopes);

            httpContext.Session.SetString("clientId", clientId);
            httpContext.Session.SetString("clientSecret", clientSecret);

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