using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup {
    public class SetupLogic {
        public List<Config> GetGlobalSettingsLogic() {
            List<Config> returnValue = new List<Config>();
            // worker count
            string workerCount = GlobalConfig.GetGlobalConfig("workerCount");
            SetupLogic.Threads threads = new SetupLogic.Threads {
                AvailableThreads = Environment.ProcessorCount
            };

            Config toReturn = new Config {
                key = "workerCount",
                value = JsonConvert.SerializeObject(threads)
            };

            if (workerCount != null) {
                threads.CurrentSetThreads = Int32.Parse(workerCount);
            }

            returnValue.Add(toReturn);

            // generate video thumbnails

            string generateVideoThumbnails =
                GlobalConfig.GetGlobalConfig("generateVideoThumbnails");
            if (generateVideoThumbnails != null) {
                returnValue.Add(new Config {
                    key = "generateVideoThumbnails",
                    value = generateVideoThumbnails
                });
            }

            // stream quality
            string streamQuality = GlobalConfig.GetGlobalConfig("streamQuality");

            if (streamQuality != null) {
                returnValue.Add(new Config {
                    key = "streamQuality",
                    value = streamQuality
                });
            }

            return returnValue;
        }

        public void SetGlobalSettingsLogic(GlobalSettings globalSettings) {
            if (globalSettings.streamQuality != null) {
                SetQualityOptions(globalSettings.streamQuality.resolution, globalSettings.streamQuality.fps);
            }

            if (globalSettings.generationEnabled != null) {
                SetGenerateVideoThumbnails(globalSettings.generationEnabled.Value);
            }

            if (globalSettings.workerCount != null) {
                UpdateThreadLimit(globalSettings.workerCount.Value);
            }
        }

        public void UpdateThreadLimit(int threadCount) {
            var setThreadCount = GlobalConfig.GetGlobalConfig("workerCount");

            if (setThreadCount != null) {
                if (threadCount == -1) {
                    GlobalConfig.RemoveGlobalConfig("workerCount");
                } else {
                    GlobalConfig.SetGlobalConfig("workerCount", threadCount.ToString());
                }
            } else if (threadCount != -1) {
                GlobalConfig.SetGlobalConfig("workerCount", threadCount.ToString());
            }
        }

        public void SetGenerateVideoThumbnails(bool generationEnabled) {
            GlobalConfig.SetGlobalConfig("generateVideoThumbnails", generationEnabled.ToString());
        }

        public void SetQualityOptions(int resolution, int fps) {
            if (resolution != 0 && fps != 0) {
                GlobalConfig.SetGlobalConfig("streamQuality", JsonConvert.SerializeObject(new SetupQualityJsonClass {
                    Resolution = resolution,
                    Fps = fps
                }));
            } else {
                GlobalConfig.RemoveGlobalConfig("streamQuality");
            }
        }

        public class Threads {
            public int AvailableThreads { get; set; }
            public int CurrentSetThreads { get; set; }
        }

        public class StreamQuality {
            public int resolution { get; set; }
            public int fps { get; set; }
        }

        public class GlobalSettings {
            public StreamQuality streamQuality { get; set; }
            public int? workerCount { get; set; }
            public bool? generationEnabled { get; set; }
        }
    }
}