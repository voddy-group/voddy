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
            using (var context = new MainDataContext()) {
                // worker count

                Config workerCount = context.Configs.FirstOrDefault(item => item.key == "workerCount");
                SetupLogic.Threads threads = new SetupLogic.Threads {
                    AvailableThreads = Environment.ProcessorCount
                };

                Config toReturn = new Config {
                    key = "workerCount",
                    value = JsonConvert.SerializeObject(threads)
                };
                
                if (workerCount != null) {
                    threads.CurrentSetThreads = Int32.Parse(workerCount.value);
                    toReturn.id = workerCount.id;
                }

                returnValue.Add(toReturn);

                // generate video thumbnails

                Config generateVideoThumbnails =
                    context.Configs.FirstOrDefault(item => item.key == "generateVideoThumbnails");
                if (generateVideoThumbnails != null) {
                    returnValue.Add(generateVideoThumbnails);
                }

                // stream quality

                Config streamQuality = context.Configs.FirstOrDefault(item => item.key == "streamQuality");

                if (streamQuality != null) {
                    returnValue.Add(streamQuality);
                }
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
            using (var context = new MainDataContext()) {
                var setThreadCount = context.Configs.FirstOrDefault(item => item.key == "workerCount");

                if (setThreadCount != null) {
                    if (threadCount == -1) {
                        context.Remove(setThreadCount);
                    } else {
                        setThreadCount.value = threadCount.ToString();
                    }
                } else if (threadCount != -1) {
                    setThreadCount = new Config {
                        key = "workerCount",
                        value = threadCount.ToString()
                    };

                    context.Configs.Add(setThreadCount);
                }

                context.SaveChanges();
            }
        }
        
        public void SetGenerateVideoThumbnails(bool generationEnabled) {
            using (var context = new MainDataContext()) {
                Config generateVideoThumbnailConfig =
                    context.Configs.FirstOrDefault(item => item.key == "generateVideoThumbnails");
                if (generateVideoThumbnailConfig != null) {
                    generateVideoThumbnailConfig.value = generationEnabled.ToString();
                } else {
                    generateVideoThumbnailConfig = new Config {
                        key = "generateVideoThumbnails",
                        value = generationEnabled.ToString()
                    };

                    context.Add(generateVideoThumbnailConfig);
                }

                context.SaveChanges();
            }
        }
        
        public void SetQualityOptions(int resolution, int fps) {
            using (var context = new MainDataContext()) {
                var qualityOption = context.Configs.FirstOrDefault(item => item.key == "streamQuality");

                if (resolution != 0 && fps != 0) {
                    if (qualityOption != null) {
                        qualityOption.value = JsonConvert.SerializeObject(new SetupQualityJsonClass {
                            Resolution = resolution,
                            Fps = fps
                        });
                    } else {
                        qualityOption = new Config {
                            key = "streamQuality",
                            value = JsonConvert.SerializeObject(new SetupQualityJsonClass {
                                Resolution = resolution,
                                Fps = fps
                            })
                        };
                        context.Add(qualityOption);
                    }
                } else {
                    if (qualityOption != null) {
                        context.Remove(qualityOption);
                    }
                }


                context.SaveChanges();
            }
        }

        public class Threads {
            public int AvailableThreads { get; set; }
            public int CurrentSetThreads { get; set; }
        }
        
        public class StreamQuality
        {
            public int resolution { get; set; }
            public int fps { get; set; }
        }

        public class GlobalSettings
        {
            public StreamQuality streamQuality { get; set; }
            public int? workerCount { get; set; }
            public bool? generationEnabled { get; set; }
        }
    }
}