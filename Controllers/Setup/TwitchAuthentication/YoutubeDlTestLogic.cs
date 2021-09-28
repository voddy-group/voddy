using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using NLog;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    public class YoutubeDlTestLogic {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        public TestResponse TestYoutubeDlLogic(string path) {
            TestResponse testResponse = new TestResponse();

            // todo needs a refactor
            string youtubeDlInstance = GlobalConfig.GetGlobalConfig("youtube-dl");
            string youtubeDlPath;
            if (youtubeDlInstance != null) {
                youtubeDlPath = youtubeDlInstance;
            } else {
                youtubeDlPath = "youtube-dl";
            }

            try {
                TestYoutubeDlPath(youtubeDlPath);
            } catch (Win32Exception e) {
                _logger.Info("Downloading Youtube-dl...");
                string downloadedPath = DownloadYoutubeDl();
                if (String.IsNullOrEmpty(downloadedPath)) {
                    testResponse.error = e.Message;
                    return testResponse;
                }

                path = downloadedPath;
            }

            if (!string.IsNullOrEmpty(path)) {
                GlobalConfig.SetGlobalConfig("youtube-dl", path);
            } else {
                GlobalConfig.SetGlobalConfig("youtube-dl", "youtube-dl");
            }

            return testResponse;
        }

        public bool UpdateYoutubeDlLogic() {
            string newYoutubeDlPath = DownloadYoutubeDl();

            if (!String.IsNullOrEmpty(newYoutubeDlPath)) {
                GlobalConfig.SetGlobalConfig("youtube-dl", newYoutubeDlPath);
                return true;
            }

            return false;
        }

        private void TestYoutubeDlPath(string youtubeDlPath) {
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

            DirectoryInfo executablesFolder = Directory.CreateDirectory(GlobalConfig.GetGlobalConfig("contentRootPath") + "executables");
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