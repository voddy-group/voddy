using System;
using System.Diagnostics;
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
                TestResponse testResponse = new TestResponse();

                using (var context = new DataContext()) {
                    Config youtubeDlInstance =
                        context.Configs.FirstOrDefault(item => item.key == "youtube-dl");

                    string youtubeDlPath;
                    if (!string.IsNullOrEmpty(path)) {
                        youtubeDlPath = path;
                    } else if (youtubeDlInstance != null) {
                        youtubeDlPath = youtubeDlInstance.value;
                    } else {
                        youtubeDlPath = "youtube-dl";
                    }
                    
                    try {
                        TestYoutubeDlPath(youtubeDlPath);
                    } catch (Exception e) {
                        testResponse.error = e.Message;
                        return testResponse;
                    }

                    if (!string.IsNullOrEmpty(path)) {
                        if (youtubeDlInstance == null) {
                            context.Configs.Add(new Config {
                                key = "youtube-dl",
                                value = path
                            });
                        } else {
                            youtubeDlInstance.value = path;
                        }
                    } else {
                        if (youtubeDlInstance == null) {
                            context.Configs.Add(new Config {
                                key = "youtube-dl",
                                value = "youtube-dl"
                            });
                        } else {
                            youtubeDlInstance.value = "youtube-dl";
                        }
                    }
                    
                    context.SaveChanges();
                }
                
                return testResponse;
            }

            private void TestYoutubeDlPath(string youtubeDlPath) {
                Console.WriteLine(youtubeDlPath);
                var processInfo = new ProcessStartInfo(youtubeDlPath, "--version");
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                var process = Process.Start(processInfo);

                process.WaitForExit();
            }

            public class TestResponse {
                public string error { get; set; }
            }
    }
}