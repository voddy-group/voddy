using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Structures;
using voddy.Data;
using voddy.Models;
using static voddy.DownloadHelpers;

namespace voddy.Controllers {
    public class StartupJobs : ControllerBase {
        private readonly ILogger<StartupJobs> _logger;
        private IBackgroundJobClient _backgroundJobClient;

        public StartupJobs(ILogger<StartupJobs> logger, IBackgroundJobClient backgroundJobClient) {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }
        
        [Queue("default")]
        public void RequeueOrphanedJobs() {
            Console.WriteLine("Checking for orphaned jobs...");
            var api = JobStorage.Current.GetMonitoringApi();
            var processingJobs = api.ProcessingJobs(0, 100);
            var servers = api.Servers();
            var orphanJobs = processingJobs.Where(j => servers.All(s => s.Name != j.Value.ServerId));
            foreach (var orphanJob in orphanJobs) {
                Console.WriteLine($"Queueing {orphanJob.Key}.");
                BackgroundJob.Requeue(orphanJob.Key);
            }
            Console.WriteLine("Done!");
        }

        public void StreamerCheckForUpdates() {
            List<Streamer> listOfStreamers = new List<Streamer>();
            using (var contrext = new DataContext()) {
                listOfStreamers = contrext.Streamers.ToList();
            }

            if (listOfStreamers.Count > 100) {
                for (int i = 0; i < listOfStreamers.Count; i = i + 100) {
                    UpdateStreamerDetails(listOfStreamers.Skip(i).Take(100).ToList());
                }
            } else {
                UpdateStreamerDetails(listOfStreamers);
            }
        }

        public void UpdateStreamerDetails(List<Streamer> listOfStreamers) {
            string listOfIds = "?id=";
            for (int i = 0; i < listOfStreamers.Count; i++) {
                if (i != listOfStreamers.Count - 1) {
                    listOfIds += listOfStreamers[i].streamerId + "&id=";
                } else {
                    listOfIds += listOfStreamers[i].streamerId;
                }
            }

            TwitchApiHelpers twitchApiHelpers = new TwitchApiHelpers();
            var response = twitchApiHelpers.TwitchRequest($"https://api.twitch.tv/helix/users{listOfIds}", Method.GET);
            var deserializedResponse = JsonConvert.DeserializeObject<UserJsonClass.User>(response.Content);
                
                
                
            for (int i = 0; i < deserializedResponse.data.Count; i++) {
                ResponseStreamer result = new ResponseStreamer {
                    streamerId = deserializedResponse.data[i].id,
                    displayName = deserializedResponse.data[i].display_name,
                    username = deserializedResponse.data[i].login,
                    thumbnailUrl = deserializedResponse.data[i].profile_image_url
                };
                    
                StreamerLogic streamerLogic = new StreamerLogic();
                streamerLogic.UpsertStreamerLogic(result, false);
            }
        }
    }
}