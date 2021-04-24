using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Streamers {
    [ApiController]
    [Route("streamer")]
    public class UpdateStreamer : ControllerBase {
        [HttpPut]
        [Route("{streamerId}/getLive")]
        public IActionResult UpdateGetLive(int streamerId, bool getLive) {
            Streamer streamer;
            using (var context = new DataContext()) {
                streamer = context.Streamers.FirstOrDefault(item => Convert.ToInt32(item.streamerId) == streamerId);
                if (streamer != null) {
                    streamer.getLive = getLive;
                    context.SaveChanges();
                    return Ok();
                }
            }

            return NotFound();
        }
    }
}