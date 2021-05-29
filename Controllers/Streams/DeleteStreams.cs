using System.IO;
using System.Linq;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RestSharp;
using voddy.Data;
using static voddy.Controllers.HandleDownloadStreams;

namespace voddy.Controllers.Streams {
    [ApiController]
    [Route("streams")]
    public class DeleteStreams : ControllerBase {
        private readonly DeleteStreamsLogic _deleteStreamsLogic;

        public DeleteStreams() {
            _deleteStreamsLogic = new DeleteStreamsLogic();
        }

        [HttpDelete]
        [Route("deleteStream")]
        public DeleteStreamReturn DeleteSingleStream(long streamId) {
            return _deleteStreamsLogic.DeleteSingleStreamLogic(streamId);
        }

        [HttpDelete]
        [Route("deleteStreams")]
        public IActionResult DeleteStreamerStreams(int streamerId) {
            _deleteStreamsLogic.DeleteStreamerStreamsLogic(streamerId);
            return Ok();
        }
    }
}