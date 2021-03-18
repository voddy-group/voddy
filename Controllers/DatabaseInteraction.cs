using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using voddy.Data;
using voddy.Models;
using static voddy.DownloadHelpers;
using Stream = voddy.Models.Stream;

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
                    if (!string.IsNullOrEmpty(body.thumbnailUrl))
                    DownloadFile(body.thumbnailUrl,
                        $"{_environment.ContentRootPath}/streamers/{body.streamerId}/thumbnail.png");

                    context.Streamers.Add(streamer);
                } else if (streamer != null) {
                    // if streamer exists then update
                }

                context.SaveChanges();
            }
        }

        [HttpGet]
        [Route("streamers")]
        public Streamers GetStreamers(int? id, string streamerId) {
            Streamers streamers = new Streamers();
            streamers.data = new List<Streamer>();
            using (var context = new DataContext()) {
                if (id != null || streamerId != null) {
                    Streamer streamer = new Streamer();
                    streamer = id != null
                        ? context.Streamers.FirstOrDefault(item => item.id == id)
                        : context.Streamers.FirstOrDefault(item => item.streamerId == streamerId);
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
        public StreamsStructure GetStreams(int? id, int? streamId, int? streamerId) {
            StreamsStructure streams = new StreamsStructure();
            streams.data = new List<Stream>();
            using (var context = new DataContext()) {
                if (id != null || streamId != null || streamerId != null) {
                    Stream stream = new Stream();
                    if (id != null) {
                        stream = context.Streams.FirstOrDefault(item => item.id == id);
                        streams.data.Add(stream);
                    } else if (streamId != null) {
                        stream = context.Streams.FirstOrDefault(item => item.streamId == streamId);
                        streams.data.Add(stream);
                    } // else if (streamerId != null) {

                    var streamList = context.Streams.ToList();
                    Console.Write(streamList.Count);
                    for (var x = 0; x < streamList.Count; x++) {
                        if (streamList[x].streamerId == streamerId) {
                            streams.data.Add(streamList[x]);
                        }
                    }
                } else {
                    streams.data = context.Streams.ToList();
                }
            }

            return streams;
        }

        [HttpDelete]
        [Route("streamer")]
        public IActionResult DeleteStreamer(string streamerId) {
            using (var context = new DataContext()) {
                var streamer = context.Streamers.FirstOrDefault(item => item.streamerId == streamerId);

                if (streamer != null) {
                    var streams = context.Streamers.AsList();
                    for (int i = 0; i < streams.Count; i++) {
                        if (streams[i].streamerId == streamerId) {
                            context.Remove(streams[i]);
                        }
                    }

                    streams.Remove(streamer);

                    Directory.Delete($"{_environment.ContentRootPath}streamers/{streamerId}", true);

                    context.SaveChanges();
                    return Ok();
                }
            }

            return NotFound();
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

    public class StreamsStructure {
        public IList<Stream> data { get; set; }
    }
}