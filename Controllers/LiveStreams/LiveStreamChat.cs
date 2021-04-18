using Microsoft.AspNetCore.Mvc;

namespace voddy.Controllers.LiveStreams {
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("liveStreamChat")]
    public class LiveStreamChat : ControllerBase {


        [HttpGet]
        [Route("download")]
        public void DownloadLiveStreamChat(string channel) {
            LiveStreamChatLogic liveStreamChatLogic = new LiveStreamChatLogic();
            //liveStreamChatLogic.DownloadLiveStreamChatLogic(channel);
        }
    }
}