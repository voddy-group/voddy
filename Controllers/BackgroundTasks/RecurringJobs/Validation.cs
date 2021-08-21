using System;
using NLog;
using RestSharp;

namespace voddy.Controllers {
    public class Validation {
        public void Validate() {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            twitchApiHelpers.TwitchRequest("https://id.twitch.tv/oauth2/validate", Method.GET);
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Validated authentication.");
        }
    }
}