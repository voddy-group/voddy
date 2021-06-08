using System;
using RestSharp;

namespace voddy.Controllers {
    public class Validation {
        public void Validate() {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            twitchApiHelpers.TwitchRequest("https://id.twitch.tv/oauth2/validate", Method.GET);
            Console.WriteLine("Validated authentication.");
        }
    }
}