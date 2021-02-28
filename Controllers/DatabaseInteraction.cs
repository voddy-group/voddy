using System;
using System.Collections.Generic;
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
                        thumbnailLocation = $"{body.streamId}/thumbnail.png"
                    };

                    CreateFolder($"{saveLocation}/{body.streamId}/");
                    DownloadThumbnail(body.thumbnailUrl, $"{saveLocation}/{body.streamId}/thumbnail.png");

                    context.Streamers.Add(streamer);
                } else if (streamer != null) {
                    // if streamer exists then update

                }

                context.SaveChanges();
            }
        }

        [HttpGet]
        [Route("streamers")]
        public Streamers GetStreamers(string id) {
            Streamers streamers = new Streamers();
            streamers.data = new List<Streamer>();
            using (var context = new DataContext()) {
                if (id != null) {
                    Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamId == id);
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