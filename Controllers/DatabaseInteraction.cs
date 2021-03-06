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

using static voddy.DownloadHelpers;

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
                Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamerId == body.streamerId);

                if (value && streamer == null) {
                    // if streamer does not exist in database and want to add
                    streamer = new Streamer {
                        streamerId = body.streamerId,
                        displayName = body.displayName,
                        username = body.username,
                        isLive = body.isLive,
                        thumbnailLocation = $"voddy/streamers/{body.streamerId}/thumbnail.png"
                    };

                    CreateFolder($"{_environment.ContentRootPath}/streamers/{body.streamerId}/");
                    DownloadFile(body.thumbnailUrl, $"{_environment.ContentRootPath}/streamers/{body.streamerId}/thumbnail.png");

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
                    streamer = id != null ? context.Streamers.FirstOrDefault(item => item.id == id) : context.Streamers.FirstOrDefault(item => item.streamerId == streamId);
                    if (streamer != null) {
                        streamers.data.Add(streamer);
                    }
                } else {
                    streamers.data = context.Streamers.ToList();
                }
            }

            return streamers;
        }

        [HttpGet]
        [Route("streams")]
        public Streams GetStreams(int? id, string streamId) {
            return new Streams();
        }
        

        private void CreateFolder(string folderLocation) {
            if (!Directory.Exists(folderLocation)) {
                Directory.CreateDirectory(folderLocation);
            }
        }
    }

    public class ResponseStreamer {
        public int id { get; set; }
        public string streamerId { get; set; }
        public string displayName { get; set; }
        public string username { get; set; }
        public bool isLive { get; set; }
        public string thumbnailUrl { get; set; }
    }

    public class Streamers {
        public IList<Streamer> data { get; set; }
    }
}