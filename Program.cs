using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog.Web;

namespace voddy {
    public class Program {
        public static void Main(string[] args) {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try {
                logger.Debug("Started.");
                
                CreateHostBuilder(args).Build().Run();
            } catch (Exception ex) {
                logger.Error(ex, "Stopped due to error.");
                throw;
            } finally {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid seg fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static string GetContentRoot() {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            return config["ContentRootPath"] ?? Directory.GetCurrentDirectory();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(GetContentRoot())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                /*.ConfigureLogging(logging => {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .UseNLog()*/;
    }
}