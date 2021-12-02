using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using Quartz;
using Quartz.Impl;
using RestSharp;
using voddy.Controllers.BackgroundTasks.RecurringJobs;
using voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs;
using voddy.Controllers.Streams;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;
using static voddy.DownloadHelpers;
using Stream = voddy.Databases.Main.Models.Stream;


namespace voddy.Controllers {
    public class StreamerLogic {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();
        
        public Streamer CreateStreamerLogic(Streamer body) {
            Streamer returnStreamer;
            using (var context = new MainDataContext()) {
                Streamer streamer = context.Streamers.FirstOrDefault(item => item.streamerId == body.streamerId);

                if (streamer == null) {
                    // if streamer does not exist in database and want to add
                    _logger.Info("Adding new streamer...");

                    string etag = "";
                    if (GlobalConfig.GetGlobalConfig("contentRootPath") != null) {
                        CreateFolder($"{GlobalConfig.GetGlobalConfig("contentRootPath")}/streamers/{body.streamerId}/");
                        if (!string.IsNullOrEmpty(body.thumbnailLocation)) {
                            etag = DownloadHelpers.DownloadFile(body.thumbnailLocation,
                                $"{GlobalConfig.GetGlobalConfig("contentRootPath")}/streamers/{body.streamerId}/thumbnail.png");
                        }
                    }


                    streamer = new Streamer {
                        streamerId = body.streamerId,
                        displayName = body.displayName,
                        username = body.username,
                        isLive = body.isLive ?? false,
                        quality = body.quality == "{\"resolution\":0,\"fps\":0}" ? null : body.quality,
                        getLive = body.getLive ?? false,
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
                //StartupJobs startupJobs = new StartupJobs();
                List<Streamer> streamers = new List<Streamer> {streamer}; //lazy
                JobHelpers.NormalJob<CheckForStreamerLiveStatusJob>("CreateStreamerUpdateLiveStatusJob", "CreateStreamerUpdateLiveStatusTrigger", QuartzSchedulers.PrimaryScheduler());
                IJobDetail job = JobBuilder.Create<UpdateStreamerDetailsJob>()
                    .WithIdentity("UpdateStreamerDetailsJob")
                    .Build();

                job.JobDataMap.Put("listOfStreamers", streamers);

                var schedulerFactory = new StdSchedulerFactory(QuartzSchedulers.PrimaryScheduler());
                IScheduler scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.Start();
            
                ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity("UpdateStreamerDetailsTrigger")
                    .StartNow()
                    .Build();

                scheduler.ScheduleJob(job, trigger);
                //}

                context.SaveChanges();
            }

            return returnStreamer;
        }

        public Streamer UpdateStreamer(Streamer body, int? id) {
            Streamer returnStreamer;
            using (var context = new MainDataContext()) {
                Streamer streamer = id == null ? context.Streamers.FirstOrDefault(item => item.streamerId == body.streamerId) : context.Streamers.FirstOrDefault(item => item.id == id);
                _logger.Info("Updating streamer...");
                if (body.streamerId != 0) {
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
                    IList<Parameter> headers = DownloadHelpers.GetHeaders(body.thumbnailLocation);
                    for (var x = 0; x < headers.Count; x++) {
                        if (headers[x].Name == "ETag") {
                            var etag = headers[x].Value;
                            if (etag != null) {
                                if (streamer.thumbnailETag != etag.ToString().Replace("\"", "")) {
                                        _logger.Info("Detected new thumbnail image, downloading...");
                                    streamer.thumbnailETag = DownloadHelpers.DownloadFile(body.thumbnailLocation,
                                        $"{GlobalConfig.GetGlobalConfig("contentRootPath")}/streamers/{body.streamerId}/thumbnail.png");
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

        public StreamerStructure GetStreamersLogic(int? id, int? streamerId) {
            StreamerStructure streamers = new StreamerStructure();
            streamers.data = new List<Streamer>();
            using (var context = new MainDataContext()) {
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
        
        public long GetStreamerVodTotalSize(string streamerId) {
            long size = 0;
            List<Stream> allStreams;
            using (var context = new MainDataContext()) {
                allStreams = context.Streams.Where(item => item.streamerId == Int32.Parse(streamerId)).ToList();
            }

            for (int i = 0; i < allStreams.Count; i++) {
                size += allStreams[i].size;
            }

            return size;
        }

        public StreamsStructure GetStreamsLogic(int? id, int? streamId, int? streamerId) {
            StreamsStructure streams = new StreamsStructure();
            streams.data = new List<Stream>();
            using (var context = new MainDataContext()) {
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

        public bool DeleteStreamer(int streamerId) {
            using (var context = new MainDataContext()) {
                var streamer = context.Streamers.FirstOrDefault(item => item.streamerId == streamerId);

                if (streamer != null) {
                    var streams = context.Streams.ToList();
                    for (int i = 0; i < streams.Count; i++) {
                        if (streams[i].streamerId == streamerId) {
                            context.Remove(streams[i]);
                        }
                    }

                    context.Remove(streamer);

                    DeleteStreamsLogic deleteStreamsLogic = new DeleteStreamsLogic();
                    deleteStreamsLogic.DeleteStreamerStreamsLogic(streamerId);
                    
                    Directory.Delete($"{GlobalConfig.GetGlobalConfig("contentRootPath")}streamers/{streamerId}/", true);

                    context.SaveChanges();
                    return true;
                }
            }

            return false;
        }
    }
    
    public class StreamerStructure {
        public IList<Streamer> data { get; set; }
    }

    public class StreamsStructure {
        public IList<Stream> data { get; set; }
    }

    public class Metadata {
        public long size { get; set; }
    }
}