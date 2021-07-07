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
        public Streamer CreateStreamerLogic(Streamer body) {
            Streamer returnStreamer;
            using (var context = new DataContext()) {
                Models.Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamerId == body.streamerId);
                var contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath");
                DownloadHelpers downloadHelpers = new DownloadHelpers();

                if (streamer == null) {
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

                    Console.WriteLine(body.quality);

                    streamer = new Models.Streamer {
                        streamerId = body.streamerId,
                        displayName = body.displayName,
                        username = body.username,
                        isLive = body.isLive ?? false,
                        getLive = body.getLive ?? false,
                        quality = body.quality,
                        thumbnailLocation = $"streamers/{body.streamerId}/thumbnail.png",
                        thumbnailETag = etag
                    };

                    context.Streamers.Add(streamer);
                    returnStreamer = streamer;
                } /*else if (streamer != null) {
                    // if streamer exists then update
                    Console.WriteLine("Updating streamer...");
                    streamer.streamerId = body.streamerId;
                    streamer.displayName = body.displayName;
                    streamer.username = body.username;
                    streamer.description = body.description;
                    streamer.viewCount = body.viewCount;
                    streamer.thumbnailLocation = $"streamers/{body.streamerId}/thumbnail.png";
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
                }*/ else {
                    //something strange has happened
                    returnStreamer = new Streamer();
                }

                //if (isNew) {
                StartupJobs startupJobs = new StartupJobs();
                List<Streamer> streamers = new List<Streamer> {streamer}; //lazy
                BackgroundJob.Enqueue(() => startupJobs.UpdateStreamerDetails(streamers));
                BackgroundJob.Enqueue(() => startupJobs.UpdateLiveStatus(streamers));
                //}

                context.SaveChanges();
            }

            return returnStreamer;
        }

        public Streamer UpdateStreamer(Streamer body, int? id) {
            DownloadHelpers downloadHelpers = new DownloadHelpers();
            Streamer returnStreamer;
            using (var context = new DataContext()) {
                Streamer streamer = id == null ? context.Streamers.FirstOrDefault(item => item.streamerId == body.streamerId) : context.Streamers.FirstOrDefault(item => item.id == id);
                var contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath");
                Console.WriteLine("Updating streamer...");
                if (body.streamerId != null) {
                    streamer.streamerId = body.streamerId;
                }

                if (body.displayName != null) {
                    streamer.displayName = body.displayName;
                }

                if (body.username != null) {
                    streamer.username = body.username;
                }

                if (body.description != null) {
                    streamer.description = body.description;
                }

                if (body.isLive != null) {
                    streamer.isLive = body.isLive;
                }

                if (body.quality != null) {
                    Console.WriteLine(body.quality);
                    if (body.quality == "{\"resolution\":0,\"fps\":0}") {
                        streamer.quality = null;
                    } else {
                        streamer.quality = body.quality;
                    }
                }

                if (body.getLive != null) {
                    streamer.getLive = body.getLive;
                }

                if (body.viewCount != null) {
                    streamer.viewCount = body.viewCount;
                }

                returnStreamer = streamer;
                if (streamer != null) {
                    context.Update(streamer);
                }

                if (body.thumbnailLocation != null) {
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