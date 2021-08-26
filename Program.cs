using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace voddy {
    public class Program {
        public static void Main(string[] args) {
            GlobalDiagnosticsContext.Set("dbFolder", SanitizePath() + "databases/");
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            //LogManager.ThrowExceptions = true;
            try {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            } catch (Exception exception) {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            } finally {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static string SanitizePath() {
            string path = ConfigurationManager.Configuration["ContentRootPath"];

            if (!path.EndsWith("/")) {
                return path + "/";
            }

            return path;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseContentRoot(SanitizePath())
                .ConfigureLogging(logging => {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                }).UseNLog(); // NLog: Setup NLog for Dependency injection
    }
}