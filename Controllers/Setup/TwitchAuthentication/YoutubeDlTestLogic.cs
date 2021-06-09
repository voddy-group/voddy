using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    public class YoutubeDlTestLogic {
        public TestResponse TestYoutubeDlLogic(string path) {
            TestResponse testResponse = new TestResponse();

            using (var context = new DataContext()) {
                Config youtubeDlInstance =
                    context.Configs.FirstOrDefault(item => item.key == "youtube-dl");

                string youtubeDlPath;
                if (!string.IsNullOrEmpty(path)) {
                    youtubeDlPath = path;
                } else if (youtubeDlInstance != null) {
                    Console.WriteLine("Youtube-dl exists.");
                    youtubeDlPath = youtubeDlInstance.value;
                } else {
                    youtubeDlPath = "youtube-dl";
                }

                try {
                    TestYoutubeDlPath(youtubeDlPath);
                } catch (Win32Exception e) {
                    Console.WriteLine("Downloading...");
                    string downloadedPath = DownloadYoutubeDl();
                    if (String.IsNullOrEmpty(downloadedPath)) {
                        testResponse.error = e.Message;
                        return testResponse;
                    }

                    path = downloadedPath;
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
                        //youtubeDlInstance.value = "youtube-dl";
                    }
                }

                context.SaveChanges();
            }

            return testResponse;
        }

        public bool UpdateYoutubeDlLogic() {
            string newYoutubeDlPath = DownloadYoutubeDl();

            if (!String.IsNullOrEmpty(newYoutubeDlPath)) {
                using (var context = new DataContext()) {
                    var existingConfig = context.Configs.FirstOrDefault(item => item.key == "youtube-dl");

                    if (existingConfig != null) {
                        existingConfig.value = newYoutubeDlPath;
                    } else {
                        existingConfig = new Config();
                        existingConfig.value = newYoutubeDlPath;
                        context.Add(existingConfig);
                    }

                    context.SaveChanges();
                }

                return true;
            }

            return false;
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

        private string DownloadYoutubeDl() {
            DownloadHelpers downloadHelpers = new DownloadHelpers();
            string contentRootPath;
            using (var context = new DataContext()) {
                contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value;
            }

            DirectoryInfo executablesFolder = Directory.CreateDirectory(contentRootPath + "executables");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
                downloadHelpers.DownloadFile("https://yt-dl.org/downloads/latest/youtube-dl",
                    executablesFolder + "/youtube-dl");
                try {
                    var processInfo = new ProcessStartInfo("chmod", $"+x {executablesFolder}/youtube-dl");
                    processInfo.CreateNoWindow = true;
                    processInfo.UseShellExecute = false;
                    processInfo.RedirectStandardError = true;
                    processInfo.RedirectStandardOutput = true;

                    var process = Process.Start(processInfo);

                    process.WaitForExit();
                    TestYoutubeDlPath(executablesFolder + "/youtube-dl");
                } catch (Win32Exception) {
                    return null;
                }

                return executablesFolder + "/youtube-dl";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                downloadHelpers.DownloadFile("https://yt-dl.org/latest/youtube-dl.exe",
                    executablesFolder + "/youtube-dl.exe");
                try {
                    TestYoutubeDlPath(executablesFolder + "/youtube-dl.exe");
                } catch (Win32Exception) {
                    return null;
                }

                return executablesFolder + "/youtube-dl.exe";
            }

            return "";
        }

        public class TestResponse {
            public string error { get; set; }
        }
    }
}