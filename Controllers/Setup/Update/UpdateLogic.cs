using System;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace voddy.Controllers.Setup.Update {
    public class UpdateLogic {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        public UpdateCheckReturn CheckForApplicationUpdates() {
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

            if (versionCompare > 0) {
                // latest version is higher
                GlobalConfig.SetGlobalConfig("updateAvailable", true.ToString());

                NotificationHub.Current.Clients.All.SendAsync("updateFound");
                updateCheckReturn.updateAvailable = true;
                _logger.Info("Update found!");
            } else {
                GlobalConfig.SetGlobalConfig("updateAvailable", false.ToString());

                updateCheckReturn.updateAvailable = false;
                _logger.Info("No updates found.");
            }

            return updateCheckReturn;
        }

        public class UpdateFile {
            public string LatestVersion { get; set; }
            public string LatestVersionLocation { get; set; }
        }
    }
}