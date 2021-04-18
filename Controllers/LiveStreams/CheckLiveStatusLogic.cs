using RestSharp;

namespace voddy.Controllers.LiveStreams {
    public class CheckLiveStatusLogic {
        public void EnableLiveStreamMonitoringLogic() {
            
        }

        public void CheckIfStreamerIsLive(int streamerId) {
            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();

            twitchApiHelpers.TwitchRequest("", Method.GET);
        }
    }
}