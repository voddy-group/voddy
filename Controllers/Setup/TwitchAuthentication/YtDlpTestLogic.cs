using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using voddy.Controllers.JSONClasses;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    public class YtDlpTestLogic {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        public void TestYtDlpLogic(string path) {
            string ytDlpInstance = GlobalConfig.GetGlobalConfig("yt-dlp");
            string ytDlpPath;
            if (ytDlpInstance != null) {
                ytDlpPath = ytDlpInstance;
            } else {
                ytDlpPath = "yt-dlp";
            }

            try {
                TestYtDlpPath(ytDlpPath);
            } catch (Win32Exception e) {
                _logger.Info("Downloading yt-dlp...");
                string downloadedPath = DownloadYtDlp();

                path = downloadedPath;
            }

            GlobalConfig.SetGlobalConfig("yt-dlp", !string.IsNullOrEmpty(path) ? path : "yt-dlp");
        }

        public bool UpdateYtDlpLogic() {
            string newYtDlpPath = DownloadYtDlp();

            if (!String.IsNullOrEmpty(newYtDlpPath)) {
                GlobalConfig.SetGlobalConfig("yt-dlp", newYtDlpPath);
                return true;
            }

            return false;
        }

        private void TestYtDlpPath(string youtubeDlPath) {
            var processInfo = new ProcessStartInfo(youtubeDlPath, "--version");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.WaitForExit();
        }

        private string DownloadYtDlp() {
            DirectoryInfo executablesFolder = Directory.CreateDirectory(GlobalConfig.GetGlobalConfig("contentRootPath") + "executables");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
                DownloadHelpers.DownloadFile("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp",
                    executablesFolder + "/yt-dlp");
                try {
                    var processInfo = new ProcessStartInfo("chmod", $"+x {executablesFolder}/yt-dlp");
                    processInfo.CreateNoWindow = true;
                    processInfo.UseShellExecute = false;
                    processInfo.RedirectStandardError = true;
                    processInfo.RedirectStandardOutput = true;

                    var process = Process.Start(processInfo);

                    process.WaitForExit();
                    TestYtDlpPath(executablesFolder + "/yt-dlp");
                } catch (Win32Exception) {
                    throw new Win32Exception("Could not chmod yt-dlp file.");
                }

                return executablesFolder + "/yt-dlp";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                DownloadHelpers.DownloadFile("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe",
                    executablesFolder + "/yt-dlp.exe");
                try {
                    TestYtDlpPath(executablesFolder + "/yt-dlp.exe");
                } catch (Win32Exception) {
                    return null;
                }

                return executablesFolder + "/yt-dlp.exe";
            }

            throw new NotSupportedException("OS not supported.");
        }

        /*private string GetDownloadUrl() {
            var client = new RestClient("https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful) {
                GitHubReleasesJsonClass.GitHubReleasesRoot releases = JsonConvert.DeserializeObject<GitHubReleasesJsonClass.GitHubReleasesRoot>(response.Content);
                GitHubReleasesJsonClass.ReleaseAsset asset = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
                    asset = releases.assets.FirstOrDefault(asset => asset.content_type == "application/octet-stream");
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    asset = releases.assets.FirstOrDefault(asset => asset.content_type == "application/vnd.microsoft.portable-executable");
                }

                if (asset != null) {
                    return asset.browser_download_url;
                }

                throw new Exception("Cannot get latest yt-dlp release. Could not find correct asset version.");
            } else {
                throw new Exception("Cannot get latest yt-dlp release. Cannot reach GitHub API.");
            }
        }*/
    }
}