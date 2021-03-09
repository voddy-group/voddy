using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
        [ApiController]
        [Route("youtubeDl")]
        public class YoutubeDlTest : ControllerBase {
            private readonly ILogger<YoutubeDlTest> _logger;
        
            public YoutubeDlTest(ILogger<YoutubeDlTest> logger) {
                _logger = logger;
            }

            [HttpGet]
            [Route("test")]
            public TestResponse TestYoutubeDl(string path) {
                YoutubeDL youtubeDl = new YoutubeDL();
                TestResponse testResponse = new TestResponse();
                
                youtubeDl.Options.GeneralOptions.Update = true;


                using (var context = new DataContext()) {
                    Executable youtubeDlInstance =
                        context.Executables.FirstOrDefault(item => item.name == "youtube-dl");

                    if (!string.IsNullOrEmpty(path)) {
                        youtubeDl.YoutubeDlPath = path;
                    } else if (youtubeDlInstance != null) {
                        youtubeDl.YoutubeDlPath = youtubeDlInstance.path;
                    }
                    
                    try {
                        youtubeDl.Download();
                    } catch (Exception e) {
                        testResponse.error = e.Message;
                        return testResponse;
                    }

                    if (!string.IsNullOrEmpty(path)) {
                        if (youtubeDlInstance == null) {
                            context.Executables.Add(new Executable {
                                name = "youtube-dl",
                                path = path
                            });
                        } else {
                            youtubeDlInstance.path = path;
                        }
                    } else {
                        if (youtubeDlInstance == null) {
                            context.Executables.Add(new Executable {
                                name = "youtube-dl",
                                path = youtubeDl.YoutubeDlPath
                            });
                        } else {
                            youtubeDlInstance.path = youtubeDl.YoutubeDlPath;
                        }
                    }
                    
                    context.SaveChanges();
                }
                
                return testResponse;
            }

            public class TestResponse {
                public string error { get; set; }
            }
    }
}