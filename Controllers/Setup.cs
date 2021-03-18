using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
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

        public class Threads {
            public int AvailableThreads { get; set; }
            public int CurrentSetThreads { get; set; }
        }
    }
}