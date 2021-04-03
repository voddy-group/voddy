using System.IO;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using voddy.Data;

namespace voddy.Controllers.Streams {
    [ApiController]
    [Route("streams")]
    public class DeleteStreams : ControllerBase {
        private readonly IWebHostEnvironment _environment;

        public DeleteStreams(IWebHostEnvironment environment) {
            _environment = environment;
        }

        [HttpDelete]
        [Route("deleteStream")]
        public IActionResult DeleteSingleStream(int streamId) {
            using (var context = new DataContext()) {
                var stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                var chat = context.Chats.Where(item => item.streamId == streamId).ToList();

                if (stream != null) {
                    if (stream.downloadJobId != null) {
                        BackgroundJob.Delete(stream.downloadJobId);
                    }

                    CleanUpStreamFiles(stream.streamId, stream.streamerId);
                    context.Remove(stream);
                    
                    if (stream.chatDownloadJobId != null) {
                        BackgroundJob.Delete(stream.chatDownloadJobId);
                        for (var x = 0; x < chat.Count; x++) {
                            context.Remove(chat[x]);
                        }
                    }
                }

                context.SaveChanges();

                return Ok();
            }
        }

        public void CleanUpStreamFiles(int streamId, int streamerId) {
            Directory.Delete($"{_environment.ContentRootPath}streamers/{streamerId}/vods/{streamId}", true);
        }

        [HttpDelete]
        [Route("deleteStreams")]
        public IActionResult DeleteStreamerStreams(int streamerId) {
            using (var context = new DataContext()) {
                var streamerStreams = context.Streams.Where(item => item.streamerId == streamerId).ToList();

                for (var x = 0; x < streamerStreams.Count; x++) {
                    DeleteSingleStream(streamerStreams[x].streamId);
                }
            }

            return Ok();
        }
    }
}