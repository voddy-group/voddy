using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using voddy.Controllers.BackgroundTasks.RecurringJobs;
using voddy.Data;
using voddy.Models;
using static voddy.DownloadHelpers;


namespace voddy.Controllers {
    public class StreamerLogic {
        public Streamer UpsertStreamerLogic(Streamer body, bool isNew) {
            Streamer returnStreamer;
            using (var context = new DataContext()) {
                Models.Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamerId == body.streamerId);
                var contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath");
                DownloadHelpers downloadHelpers = new DownloadHelpers();

                if (isNew && streamer == null) {
                    // if streamer does not exist in database and want to add
                    Console.WriteLine("Adding new streamer...");

                    string etag = "";
                    if (contentRootPath != null) {
                        CreateFolder($"{contentRootPath.value}/streamers/{body.streamerId}/");
                        if (!string.IsNullOrEmpty(body.thumbnailLocation)) {
                            etag = downloadHelpers.DownloadFile(body.thumbnailLocation,
                                $"{contentRootPath.value}/streamers/{body.streamerId}/thumbnail.png");
                        }
                    }

                    streamer = new Models.Streamer {
                        streamerId = body.streamerId,
                        displayName = body.displayName,
                        username = body.username,
                        isLive = body.isLive,
                        thumbnailLocation = $"voddy/streamers/{body.streamerId}/thumbnail.png",
                        thumbnailETag = etag
                    };

                    context.Streamers.Add(streamer);
                    returnStreamer = streamer;
                } else if (streamer != null) {
                    // if streamer exists then update
                    Console.WriteLine("Updating streamer...");
                    streamer.streamerId = body.streamerId;
                    streamer.displayName = body.displayName;
                    streamer.username = body.username;
                    streamer.isLive = body.isLive;
                    streamer.description = body.description;
                    streamer.viewCount = body.viewCount;
                    streamer.thumbnailLocation = $"voddy/streamers/{body.streamerId}/thumbnail.png";
                    returnStreamer = streamer;

                    IList<Parameter> headers = downloadHelpers.GetHeaders(body.thumbnailLocation);
                    for (var x = 0; x < headers.Count; x++) {
                        if (headers[x].Name == "ETag") {
                            var etag = headers[x].Value;
                            if (etag != null) {
                                if (streamer.thumbnailETag != etag.ToString().Replace("\"", "")) {
                                    if (contentRootPath != null)
                                        Console.WriteLine("Detected new thumbnail image, downloading...");
                                    streamer.thumbnailETag = downloadHelpers.DownloadFile(body.thumbnailLocation,
                                        $"{contentRootPath.value}/streamers/{body.streamerId}/thumbnail.png");
                                }
                            }
                        }
                    }
                } else {
                    //something strange has happened
                    returnStreamer = new Streamer();
                }

                if (isNew) {
                    StartupJobs startupJobs = new StartupJobs();
                    List<Streamer> streamers = new List<Streamer> {streamer}; //lazy
                    BackgroundJob.Enqueue(() => startupJobs.UpdateStreamerDetails(streamers));
                }
                context.SaveChanges();
            }

            return returnStreamer;
        }

        private void CreateFolder(string folderLocation) {
            if (!Directory.Exists(folderLocation)) {
                Directory.CreateDirectory(folderLocation);
            }
        }
    }
}