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
        public void UpsertStreamer([FromBody] Streamer body, bool isNew) {
            StreamerLogic streamerLogic = new StreamerLogic();
            streamerLogic.UpsertStreamerLogic(body, isNew);
        }

        [HttpGet]
        [Route("streamers")]
        public StreamerStructure GetStreamers(int? id, string streamerId) {
            StreamerStructure streamers = new StreamerStructure();
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
        [Route("streamerMeta")]
        public Metadata GetStreamerMetadata(string streamerId) {
            /*
             * Returns local metadata about a streamer such as total size of vods on hdd. May expand later.
             */
            return new Metadata {size = GetStreamerVodTotalSize(streamerId)};
        }

        public long GetStreamerVodTotalSize(string streamerId) {
            long size = 0;
            List<Stream> allStreams;
            using (var context = new DataContext()) {
                allStreams = context.Streams.Where(item => item.streamerId == Int32.Parse(streamerId)).ToList();
            }

            for (int i = 0; i < allStreams.Count; i++) {
                size += allStreams[i].size;
            }

            return size;
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
                    var streams = context.Streams.AsList();
                    for (int i = 0; i < streams.Count; i++) {
                        if (streams[i].streamerId == Int32.Parse(streamerId)) {
                            context.Remove(streams[i]);
                        }
                    }

                    context.Remove(streamer);

                    Directory.Delete($"{_environment.ContentRootPath}streamers/{streamerId}", true);

                    context.SaveChanges();
                    return Ok();
                }
            }

            return NotFound();
        }

        [HttpPut]
        [Route("streamer")]
        public IActionResult UpdateStreamer([FromBody] Models.Streamer streamer) {
            using (var context = new DataContext()) {
                var dbStreamer = context.Streamers.FirstOrDefault(item => item.streamerId == streamer.streamerId);

                if (dbStreamer != null) {
                    context.Update(dbStreamer).CurrentValues.SetValues(streamer);
                }

                context.SaveChanges();
            }

            return Ok();
        }
    }

    public class ResponseStreamer {
        public int id { get; set; }
        public string streamerId { get; set; }
        public string displayName { get; set; }
        public string username { get; set; }
        public bool isLive { get; set; }
        public string thumbnailUrl { get; set; }
        public string thumbnailETag { get; set; }
    }

    public class StreamerStructure {
        public IList<Models.Streamer> data { get; set; }
    }

    public class StreamsStructure {
        public IList<Stream> data { get; set; }
    }

    public class Metadata {
        public long size { get; set; }
    }
}