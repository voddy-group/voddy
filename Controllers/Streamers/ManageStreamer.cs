using Microsoft.AspNetCore.Mvc;
using voddy.Databases.Main.Models;

namespace voddy.Controllers {
    [ApiController]
    [Route("streamer")]
    public class ManageStreamer : ControllerBase {
        private StreamerLogic _streamerLogic { get; set; }
        public ManageStreamer() {
            
        }
        
        [HttpGet]
        [Route("list")]
        public StreamerStructure GetStreamers(int? id, int? streamerId) {
            /*
             * Get streamer(s) from the db.
             */
            return _streamerLogic.GetStreamersLogic(id, streamerId);
        }
        
        [HttpPost]
        [Route("create")]
        public Streamer CreateStreamer([FromBody] Streamer body) {
            /*
             *  Create a streamer in the db.
             */
            return _streamerLogic.CreateStreamerLogic(body);
        }
        
        [HttpPut]
        [Route("update")]
        public Streamer UpdateStreamer([FromBody] Streamer body, int id) {
            /*
             * Update a streamer in the db.
             */
            return _streamerLogic.UpdateStreamer(body, id);
        }
        
        [HttpGet]
        [Route("meta")]
        public Metadata GetStreamerMetadata(string streamerId) {
            /*
             * Returns local metadata about a streamer such as total size of vods on hdd. May expand later.
             */
            return new Metadata {size = _streamerLogic.GetStreamerVodTotalSize(streamerId)};
        }
        
        [HttpDelete]
        [Route("delete")]
        public IActionResult DeleteStreamer(int streamerId) {
            bool returnValue = _streamerLogic.DeleteStreamer(streamerId);
            if (returnValue) {
                return Ok();
            } else {
                return NotFound();
            }
        }
        
        [HttpGet]
        [Route("streams")]
        public StreamsStructure GetStreams(int? id, int? streamId, int? streamerId) {
            /*
             * Get all streams of a streamer from the db. Does not get non-downloaded streams. Currently not in use.
             */
            return _streamerLogic.GetStreamsLogic(id, streamId, streamerId);
        }
    }
}