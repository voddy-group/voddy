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
                var chat = context.Chats.FirstOrDefault(item => item.streamId == streamId);

                if (stream != null) {
                    if (stream.downloadJobId != null) {
                        BackgroundJob.Delete(stream.downloadJobId);
                    }

                    CleanUpStreamFiles(stream.streamId, stream.streamerId);
                    context.Remove(stream);
                }

                if (chat != null) {
                    if (chat.downloadJobId != null) {
                        BackgroundJob.Delete(chat.downloadJobId);
                    }

                    context.Remove(chat);
                }

                context.SaveChanges();

                return Ok();
            }
        }

        public void CleanUpStreamFiles(int streamId, int streamerId) {
            Directory.Delete($"{_environment.ContentRootPath}streamers/{streamerId}/vods/{streamId}", true);
        }
    }
}