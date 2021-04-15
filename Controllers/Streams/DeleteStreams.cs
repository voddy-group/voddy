using System.IO;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using voddy.Data;
using static voddy.Controllers.HandleDownloadStreams;

namespace voddy.Controllers.Streams {
    [ApiController]
    [Route("streams")]
    public class DeleteStreams : ControllerBase {
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<NotificationHub> _hubContext;

        public DeleteStreams(IWebHostEnvironment environment, IHubContext<NotificationHub> hubContext) {
            _environment = environment;
            _hubContext = hubContext;
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

                HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
                _hubContext.Clients.All.SendAsync("ReceiveMessage", handleDownloadStreamsLogic.CheckForDownloadingStreams());

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