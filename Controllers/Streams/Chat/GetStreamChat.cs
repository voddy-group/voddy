using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace voddy.Controllers.Streams.Chat {
    [ApiController]
    [Route("chat")]
    public class GetStreamChat : ControllerBase {
        [HttpGet]
        [Route("{streamId}")]
        public GetChatReturnJson getChat(long streamId) {
            GetStreamChatLogic getStreamChatLogic = new GetStreamChatLogic();
            return new GetChatReturnJson {
                url = getStreamChatLogic.getChatLogic(streamId)
            };
        }
    }

    public class GetChatReturnJson {
        public string url { get; set; }
    }
}