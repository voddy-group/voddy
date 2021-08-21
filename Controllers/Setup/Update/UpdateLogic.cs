using System;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.Update {
    public class UpdateLogic {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();
        
        public UpdateCheckReturn CheckForUpdates() {
            var client = new RestClient("https://raw.githubusercontent.com/voddy-group/Update/master/LatestUpdate.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var updateFile = JsonConvert.DeserializeObject<UpdateFile>(response.Content);
            UpdateCheckReturn updateCheckReturn = new UpdateCheckReturn {
                latestVersion = updateFile.LatestVersion,
            };
            Version parsedLatestVersion = Version.Parse(updateFile.LatestVersion);
            int versionCompare = parsedLatestVersion.CompareTo(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            using (var context = new MainDataContext()) {
                var existingConfig = context.Configs.FirstOrDefault(item => item.key == "updateAvailable");
                Config config = new Config();
                config.key = "updateAvailable";
                if (versionCompare > 0) { // latest version is higher
                    if (existingConfig == null) {
                        config.value = true.ToString();

                        context.Add(config);
                    } else {
                        existingConfig.value = true.ToString();
                    }

                    NotificationHub.Current.Clients.All.SendAsync("updateFound");
                    updateCheckReturn.updateAvailable = true;
                    _logger.Info("Update found!");
                } else {
                    if (existingConfig == null) {
                        config.value = false.ToString();

                        context.Add(config);
                    } else {
                        existingConfig.value = false.ToString();
                    }

                    updateCheckReturn.updateAvailable = false;
                    _logger.Info("No updates found.");
                }

                context.SaveChanges();
            }

            return updateCheckReturn;
        }

        public bool GetUpdatesLogic() {
            bool returnValue = false;
            using (var context = new MainDataContext()) {
                var update = context.Configs.FirstOrDefault(item => item.key == "updateAvailable");
                if (update != null && Boolean.Parse(update.value)) {
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public class UpdateFile {
            public string LatestVersion { get; set; }
            public string LatestVersionLocation { get; set; }
        }
    }
}