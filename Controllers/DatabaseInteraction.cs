using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
    [ApiController]
    [Route("database")]
    public class DatabaseInteractions : ControllerBase {
        private readonly ILogger<TwitchApi> _logger;
        private readonly IWebHostEnvironment _environment;
        
        public DatabaseInteractions(ILogger<TwitchApi> logger, IWebHostEnvironment environment) {
            _logger = logger;
            _environment = environment;
        }

        [HttpPost]
        [Route("streamer")]
        public void UpsertStreamer([FromBody] ResponseStreamer body, bool value) {
            using (var context = new DataContext()) {
                Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamId == body.streamId);

                if (value && streamer == null) {
                    // if streamer does not exist in database and want to add
                    streamer = new Streamer {
                        streamId = body.streamId,
                        displayName = body.displayName,
                        username = body.username,
                        isLive = body.isLive,
                        thumbnailLocation = $"voddy/streamers/{body.streamId}/thumbnail.png"
                    };

                    CreateFolder($"{_environment.ContentRootPath}/streamers/{body.streamId}/");
                    DownloadThumbnail(body.thumbnailUrl, $"{_environment.ContentRootPath}/streamers/{body.streamId}/thumbnail.png");

                    context.Streamers.Add(streamer);
                } else if (streamer != null) {
                    // if streamer exists then update
                }

                context.SaveChanges();
            }
        }

        [HttpGet]
        [Route("streamers")]
        public Streamers GetStreamers(int? id, string streamId) {
            Streamers streamers = new Streamers();
            streamers.data = new List<Streamer>();
            using (var context = new DataContext()) {
                if (id != null || streamId != null) {
                    Streamer streamer = new Streamer();
                    streamer = id != null ? context.Streamers.FirstOrDefault(item => item.id == id) : context.Streamers.FirstOrDefault(item => item.streamId == streamId);
                    if (streamer != null) {
                        streamers.data.Add(streamer);
                    }
                } else {
                    streamers.data = context.Streamers.ToList();
                }
            }

            return streamers;
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
        public int id { get; set; }
        public string streamId { get; set; }
        public string displayName { get; set; }
        public string username { get; set; }
        public bool isLive { get; set; }
        public string thumbnailUrl { get; set; }
    }

    public class Streamers {
        public IList<Streamer> data { get; set; }
    }
}