using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using RestSharp;
using voddy.Data;
using voddy.Models;
using static voddy.DownloadHelpers;


namespace voddy.Controllers {
    public class StreamerLogic {
        public void UpsertStreamerLogic(ResponseStreamer body, bool isNew) {
            using (var context = new DataContext()) {
                Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamerId == body.streamerId);
                var contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath");

                if (isNew && streamer == null) {
                    // if streamer does not exist in database and want to add
                    Console.WriteLine("Adding new streamer...");

                    string etag = "";
                    if (contentRootPath != null) {
                        CreateFolder($"{contentRootPath.value}/streamers/{body.streamerId}/");
                        if (!string.IsNullOrEmpty(body.thumbnailUrl)) {
                            etag = DownloadFile(body.thumbnailUrl,
                                $"{contentRootPath.value}/streamers/{body.streamerId}/thumbnail.png");
                        }
                    }

                    streamer = new Streamer {
                        streamerId = body.streamerId,
                        displayName = body.displayName,
                        username = body.username,
                        isLive = body.isLive,
                        thumbnailLocation = $"voddy/streamers/{body.streamerId}/thumbnail.png",
                        thumbnailETag = etag
                    };

                    context.Streamers.Add(streamer);
                } else if (streamer != null) {
                    // if streamer exists then update
                    Console.WriteLine("Updating streamer...");
                    streamer.streamerId = body.streamerId;
                    streamer.displayName = body.displayName;
                    streamer.username = body.username;
                    streamer.isLive = body.isLive;
                    streamer.thumbnailLocation = $"voddy/streamers/{body.streamerId}/thumbnail.png";

                    IList<Parameter> headers = GetHeaders(body.thumbnailUrl);
                    for (var x = 0; x < headers.Count; x++) {
                        if (headers[x].Name == "ETag") {
                            var etag = headers[x].Value;
                            if (etag != null) {
                                if (streamer.thumbnailETag != etag.ToString().Replace("\"", "")) {
                                    if (contentRootPath != null)
                                        Console.WriteLine("Detected new thumbnail image, downloading...");
                                    streamer.thumbnailETag = DownloadFile(body.thumbnailUrl,
                                        $"{contentRootPath.value}/streamers/{body.streamerId}/thumbnail.png");
                                }
                            }
                        }
                    }
                }

                context.SaveChanges();
            }
        }

        private void CreateFolder(string folderLocation) {
            if (!Directory.Exists(folderLocation)) {
                Directory.CreateDirectory(folderLocation);
            }
        }
    }
}