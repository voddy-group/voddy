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
using voddy.Controllers.Setup.Update;
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
                string downloadedPath = UpdateYtDlp.DownloadYtDlp();

                path = downloadedPath;
            }

            GlobalConfig.SetGlobalConfig("yt-dlp", !string.IsNullOrEmpty(path) ? path : "yt-dlp");
        }

        public static void TestYtDlpPath(string youtubeDlPath) {
            var processInfo = new ProcessStartInfo(youtubeDlPath, "--version");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.WaitForExit();
        }
    }
}