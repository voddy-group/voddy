#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.JSONClasses;
using voddy.Controllers.Notifications;
using voddy.Controllers.Setup.TwitchAuthentication;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Setup.Update {
    public class UpdateYtDlp {
        public Task CheckForYtDlpUpdates() {
            Version? currentVersion = null;
            string? dbVersion = GlobalConfig.GetGlobalConfig("yt-dlpVersion");
            if (dbVersion == null) {
                // current yt-dlp (if it exists) does not have the version number in the db.
                string? ytDlpInstallLocation = GlobalConfig.GetGlobalConfig("yt-dlp");

                if (ytDlpInstallLocation != null) {
                    // yt-dlp is installed, need to get the current version.
                    try {
                        currentVersion = GetCurrentYtDlpVersion(ytDlpInstallLocation);
                    } catch (Exception e) {
                        // ignored, probably an error due to yt-dlp not existing.
                    }

                    if (currentVersion is not null) GlobalConfig.SetGlobalConfig("yt-dlpVersion", currentVersion.ToString());
                }
            } else {
                currentVersion = Version.Parse(dbVersion);
            }

            Version latestVersion = GetLatestYtDlpVersion();
            if (currentVersion != null && currentVersion >= latestVersion) {
                return Task.CompletedTask;
            }

            NotificationLogic.CreateNotification("yt-dlpUpdate", Severity.Info, Position.Top, $"New yt-dlp update! Current version: {(currentVersion != null ? currentVersion : "unknown")}; latest: {latestVersion}.", "/settings/setup");
            GlobalConfig.SetGlobalConfig("yt-dlpUpdate", true.ToString());
            return Task.CompletedTask;
        }

        public static string DownloadYtDlp() {
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
                    YtDlpTestLogic.TestYtDlpPath(executablesFolder + "/yt-dlp");
                } catch (Win32Exception) {
                    throw new Win32Exception("Could not chmod yt-dlp file.");
                }

                GlobalConfig.SetGlobalConfig("yt-dlp", executablesFolder + "/yt-dlp");
                GlobalConfig.SetGlobalConfig("yt-DlpVersion", GetLatestYtDlpVersion().ToString());

                return executablesFolder + "/yt-dlp";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                DownloadHelpers.DownloadFile("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe",
                    executablesFolder + "/yt-dlp.exe");
                try {
                    YtDlpTestLogic.TestYtDlpPath(executablesFolder + "/yt-dlp.exe");
                } catch (Win32Exception) {
                    return null;
                }

                GlobalConfig.SetGlobalConfig("yt-dlp", executablesFolder + "/yt-dlp.exe");
                GlobalConfig.SetGlobalConfig("yt-DlpVersion", GetLatestYtDlpVersion().ToString());

                return executablesFolder + "/yt-dlp.exe";
            }

            throw new NotSupportedException("OS not supported.");
        }

        public static Version GetLatestYtDlpVersion() {
            var client = new RestClient("https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful) {
                GitHubReleasesJsonClass.GitHubReleasesRoot latest = JsonConvert.DeserializeObject<GitHubReleasesJsonClass.GitHubReleasesRoot>(response.Content);

                if (latest != null &&
                    !latest.draft &&
                    !latest.prerelease) {
                    return Version.Parse((ReadOnlySpan<char>)latest.tag_name);
                }

                throw new Exception("Cannot get latest yt-dlp release. Could not find correct asset version.");
            } else {
                throw new Exception("Cannot get latest yt-dlp release. Cannot reach GitHub API.");
            }
        }

        public static Version GetCurrentYtDlpVersion(string youtubeDlPath) {
            var processInfo = new ProcessStartInfo(youtubeDlPath, "--version");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            string version = null;

            process.OutputDataReceived += (_, e) => {
                if (!String.IsNullOrEmpty(e.Data)) {
                    version = e.Data;
                }
            };

            process.BeginOutputReadLine();

            process.WaitForExit();

            if (version != null) return Version.Parse(version);

            throw new Exception("Could not parse current version.");
        }
    }
}