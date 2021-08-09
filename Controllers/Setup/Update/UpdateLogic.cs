using System;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.Update {
    public class UpdateLogic {
        public bool CheckForUpdates() {
            var client = new RestClient("https://raw.githubusercontent.com/voddy-group/Update/master/LatestUpdate.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var updateFile = JsonConvert.DeserializeObject<UpdateFile>(response.Content);
            bool updateFound;

            using (var context = new MainDataContext()) {
                var existingConfig = context.Configs.FirstOrDefault(item => item.key == "updateAvailable");
                Config config = new Config();
                config.key = "updateAvailable";
                if (updateFile.LatestVersion !=
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()) {
                    if (existingConfig == null) {
                        config.value = true.ToString();

                        context.Add(config);
                    } else {
                        existingConfig.value = true.ToString();
                    }

                    updateFound = true;
                    Console.WriteLine("Update found!");
                } else {
                    if (existingConfig == null) {
                        config.value = false.ToString();

                        context.Add(config);
                    } else {
                        existingConfig.value = false.ToString();
                    }

                    updateFound = false;
                    Console.WriteLine("No updates found.");
                }

                context.SaveChanges();
            }

            return updateFound;
        }

        public class UpdateFile {
            public string LatestVersion { get; set; }
            public string LatestVersionLocation { get; set; }
        }
    }
}