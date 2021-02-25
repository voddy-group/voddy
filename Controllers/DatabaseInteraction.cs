using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestSharp;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("database")]
    public class DatabaseInteractions : ControllerBase {
        private readonly ILogger<TwitchApi> _logger;
        private string saveLocation = "/var/lib/voddy";

        public DatabaseInteractions(ILogger<TwitchApi> logger) {
            _logger = logger;
        }

        [HttpPatch]
        [Route("streamer")]
        public void UpdateStreamer([FromBody] ResponseStreamer body, bool value) {
            using (var context = new DataContext()) {
                Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamId == body.streamId);

                if (value && streamer == null) {
                    // if streamer does not exist in database and want to add
                    streamer = new Streamer {
                        streamId = body.streamId,
                        displayName = body.displayName,
                        username = body.username,
                        isLive = body.isLive,
                        thumbnailLocation = $"{body.streamId}/thumbnail.png"
                    };

                    CreateFolder($"{saveLocation}/{body.streamId}/");
                    DownloadThumbnail(body.thumbnailUrl, $"{saveLocation}/{body.streamId}/thumbnail.png");

                    context.Streamers.Add(streamer);
                }

                context.SaveChanges();
            }
        }
        

        public void DownloadThumbnail(string url, string location) {
            HttpClient client = new HttpClient();
            var contentBytes = client.GetByteArrayAsync(new Uri(url)).Result;
            MemoryStream stream = new MemoryStream(contentBytes);
            FileStream file = new FileStream(location, FileMode.Create, FileAccess.Write);
            stream.WriteTo(file);
            file.Close();
            stream.Close();
        }

        private void CreateFolder(string folderLocation) {
            if (!Directory.Exists(folderLocation)) {
                Directory.CreateDirectory(folderLocation);
            }
        }
    }

    public class ResponseStreamer {
        public string streamId { get; set; }
        public string displayName { get; set; }
        public string username { get; set; }
        public bool isLive { get; set; }
        public string thumbnailUrl { get; set; }
    }
}