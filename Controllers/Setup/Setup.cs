using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using voddy.Controllers.Structures;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Setup {
    [ApiController]
    [Route("setup")]
    public class Setup : ControllerBase {
        [HttpGet]
        [Route("threads")]
        public Threads GetThreads() {
            var returnValue = new Threads();
            using (var context = new DataContext()) {
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
            using (var context = new DataContext()) {
                var setThreadCount = context.Configs.FirstOrDefault(item => item.key == "workerCount");

                if (setThreadCount != null) {
                    if (threadCount == -1) {
                        context.Remove(setThreadCount);
                    } else {
                        setThreadCount.value = threadCount.ToString();
                    }
                } else if (threadCount != -1){
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

        [HttpGet]
        [Route("quality")]
        public string GetCurrentQualityOptions() {
            using (var context = new DataContext()) {
                var qualityOption = context.Configs.FirstOrDefault(item => item.key == "streamQuality");

                return qualityOption?.value;
            }
        }

        [HttpPut]
        [Route("quality")]
        public IActionResult SetQualityOptions(int resolution, int fps) {
            using (var context = new DataContext()) {
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