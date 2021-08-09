using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup {
    [ApiController]
    [Route("setup")]
    public class Setup : ControllerBase {
        [HttpGet]
        [Route("threads")]
        public Threads GetThreads() {
            var returnValue = new Threads();
            using (var context = new MainDataContext()) {
                var setThreadCount = context.Configs.FirstOrDefault(item => item.key == "workerCount");

                if (setThreadCount != null) {
                    returnValue.CurrentSetThreads = Int32.Parse(setThreadCount.value);
                }
            }

            returnValue.AvailableThreads = Environment.ProcessorCount;
            return returnValue;
        }

        [HttpPut]
        [Route("threads")]
        public IActionResult UpdateThreadLimit(int threadCount) {
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

                return Ok();
            }
        }

        [HttpPut]
        [Route("generateVideoThumbnails")]
        public IActionResult SetGenerateVideoThumbnails(bool generationEnabled) {
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

            return Ok();
        }

        [HttpGet]
        [Route("globalSettings")]
        public List<Config> GetGlobalSettings() {
            List<Config> returnValue = new List<Config>();
            using (var context = new MainDataContext()) {
                // worker count

                Config workerCount = context.Configs.FirstOrDefault(item => item.key == "workerCount");
                Threads threads = new Threads {
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

        private void AddToConfigList(MainDataContext context, List<Config> list, string[] keyList) {
            for (int x = 0; x < keyList.Length; x++) {
                Config config = context.Configs.FirstOrDefault(item => item.key == keyList[x]);
                if (config != null) {
                    list.Add(config);
                }
            }
        }

        [HttpGet]
        [Route("quality")]
        public string GetCurrentQualityOptions() {
            using (var context = new MainDataContext()) {
                var qualityOption = context.Configs.FirstOrDefault(item => item.key == "streamQuality");

                return qualityOption?.value;
            }
        }

        [HttpPut]
        [Route("quality")]
        public IActionResult SetQualityOptions(int resolution, int fps) {
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
                return Ok();
            }
        }

        public class Threads {
            public int AvailableThreads { get; set; }
            public int CurrentSetThreads { get; set; }
        }
    }
}