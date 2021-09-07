using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NLog;
using Quartz;
using voddy.Controllers;
using voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs;
using voddy.Databases.Chat;
using voddy.Databases.Logs;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;


namespace voddy {
    public class Startup {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllersWithViews();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddMvc();
            //services.AddDbContext<MainDataContext>();
            services.AddEntityFrameworkSqlite().AddDbContext<MainDataContext>();

            services.AddEntityFrameworkSqlite().AddDbContext<ChatDataContext>();

            services.AddEntityFrameworkSqlite().AddDbContext<LogDataContext>();


            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });

            // hangfire
            /*new DirectoryInfo($"{SanitizePath()}databases").Create();
            new LiteDatabase($"{SanitizePath()}databases/hangfireDb.db");
            string hangfireDbSelection = ConfigurationManager.Configuration["HangfireDbSelection"];
            if (hangfireDbSelection == "LiteDb") {
                services.AddHangfire(c =>
                    c.UseLiteDbStorage($"{SanitizePath()}databases/hangfireDb.db", new LiteDbStorageOptions {
                        InvisibilityTimeout = TimeSpan.FromDays(1) // stop jobs from restarting after 30 minutes
                    }));
            } else if (hangfireDbSelection == "PostgreSQL") {
                services.AddHangfire(c =>
                    c.UsePostgreSqlStorage(ConfigurationManager.Configuration["ConnectionStrings:PostgreSQLHangfireConnection"], new PostgreSqlStorageOptions() {
                        InvisibilityTimeout = TimeSpan.FromDays(1) // stop jobs from restarting after 30 minutes
                    }));
            }*/

            services.AddSignalR();
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder => builder.AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins("https://localhost:5001")
                .AllowCredentials()));

            // quartz
            /*services.AddQuartz(QuartzSchedulers.PrimaryScheduler(), item => item.UsePersistentStore(e => {
                e.UseSQLite($"Data Source={QuartzDbCheck()}");
                e.UseJsonSerializer();
            }));*/
            services.AddQuartz(QuartzSchedulers.PrimaryScheduler(), item => {
                item.UseMicrosoftDependencyInjectionJobFactory();
                item.UsePersistentStore(e => {
                    e.UseSQLite($"Data Source={QuartzDbCheck()}");
                    e.UseJsonSerializer();
                });

                var checkForLiveStatusJobKey = new JobKey("CheckForStreamerLiveStatusJob");
                var checkForStreamerUpdates = new JobKey("CheckForStreamerUpdatesJob");
                var trimLogs = new JobKey("TrimLogsJob");
                var removeTemp = new JobKey("RemoveTempJob");
                var checkStreamFileExists = new JobKey("CheckStreamFileExistsJob");
                var refreshValidation = new JobKey("RefreshValidationJob");
                var checkForUpdates = new JobKey("CheckForUpdatesJob");
                var chatDatabaseBackup = new JobKey("ChatDatabaseBackupJob");
                var mainDatabaseBackup = new JobKey("MainDatabaseBackupJob");

                item.AddJob<CheckForStreamerLiveStatusJob>(jobConfigurator => 
                    jobConfigurator.WithIdentity(checkForLiveStatusJobKey));
                item.AddJob<StreamerCheckForUpdatesJob>(jobConfigurator =>
                    jobConfigurator.WithIdentity(checkForStreamerUpdates));
                item.AddJob<TrimLogsJob>(jobConfigurator =>
                    jobConfigurator.WithIdentity(trimLogs));
                item.AddJob<RemoveTempJob>(jobConfigurator =>
                    jobConfigurator.WithIdentity(removeTemp));
                item.AddJob<CheckStreamFileExistsJob>(jobConfigurator =>
                    jobConfigurator.WithIdentity(checkStreamFileExists));
                item.AddJob<RefreshValidationJob>(jobConfigurator =>
                    jobConfigurator.WithIdentity(refreshValidation));
                item.AddJob<CheckForUpdatesJob>(jobConfigurator =>
                    jobConfigurator.WithIdentity(checkForUpdates));
                item.AddJob<DatabaseBackupJob>(jobConfigurator => {
                    jobConfigurator.WithIdentity(chatDatabaseBackup);
                    jobConfigurator.UsingJobData("database", "chatDb");
                });
                item.AddJob<DatabaseBackupJob>(jobConfigurator => {
                    jobConfigurator.WithIdentity(mainDatabaseBackup);
                    jobConfigurator.UsingJobData("database", "mainDb");
                });

                item.AddTrigger(trigger =>
                    trigger.ForJob(checkForLiveStatusJobKey)
                        .WithIdentity("CheckForLiveStatusJob")
                        .WithCronSchedule("0 0/1 * 1/1 * ? *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(checkForStreamerUpdates)
                        .WithIdentity("StreamerCheckForUpdatesJob")
                        .WithCronSchedule("0 0 0 ? * MON *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(trimLogs)
                        .WithIdentity("TrimLogsJob")
                        .WithCronSchedule("0 0 0 ? * MON *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(removeTemp)
                        .WithIdentity("RemoveTempJob")
                        .WithCronSchedule("0 0/1 * 1/1 * ? *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(checkStreamFileExists)
                        .WithIdentity("CheckStreamFileExistsJob")
                        .WithCronSchedule("0 0/5 * 1/1 * ? *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(refreshValidation)
                        .WithIdentity("RefreshValidationJob")
                        .WithCronSchedule("0 0/5 * 1/1 * ? *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(checkForUpdates)
                        .WithIdentity("CheckForUpdatesJob")
                        .WithCronSchedule("0 0 12 1/1 * ? *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(chatDatabaseBackup)
                        .WithIdentity("ChatDatabaseBackupJob")
                        .WithCronSchedule("0 0 0 ? * MON *"));
                item.AddTrigger(trigger =>
                    trigger.ForJob(mainDatabaseBackup)
                        .WithIdentity("MainDatabaseBackupJob")
                        .WithCronSchedule("0 0 0 ? * MON *"));
            });

            services.AddQuartzHostedService(item => { item.WaitForJobsToComplete = false; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //var hbctx = app.ApplicationServices.GetRequiredService<IHubContext<NotificationHub>>();
            NotificationHub.Current = app.ApplicationServices.GetService<IHubContext<NotificationHub>>();


            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions {
                FileProvider = new PhysicalFileProvider(env.ContentRootPath),
                //RequestPath = "/voddy"
            });
            app.UseSpaStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                //endpoints.MapHangfireDashboard();
                endpoints.MapHub<NotificationHub>("/notificationhub");
            });

            app.UseSpa(spa => {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment()) {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            // create databases on first run

            using (var scope = app.ApplicationServices.CreateScope()) {
                using (var mainDataContext = scope.ServiceProvider.GetService<MainDataContext>()) {
                    _logger.Info("Migrating Main db.");
                    mainDataContext.Database.Migrate();
                }

                using (var chatDataContext = scope.ServiceProvider.GetService<ChatDataContext>()) {
                    _logger.Info("Migrating Chat db.");
                    chatDataContext.Database.Migrate();
                }

                using (var logDataContext = scope.ServiceProvider.GetService<LogDataContext>()) {
                    _logger.Info("Migrating Log db.");
                    logDataContext.Database.Migrate();
                }
            }

            var path = SanitizePath();
            AddContentRootPathToDatabase(path);

            //hangfire
            /*var options = new BackgroundJobServerOptions();
            using (var context = new MainDataContext()) {
                var workerCountValue = context.Configs.FirstOrDefault(item => item.key == "workerCount");

                if (workerCountValue != null) {
                    options.WorkerCount = Int32.Parse(workerCountValue.value);
                }
            }

            options.Queues = new[] { "default" };
            options.ServerName = "MainServer";

            app.UseHangfireServer(options);

            var options2 = new BackgroundJobServerOptions {
                Queues = new[] { "single" },
                ServerName = "Single",
                WorkerCount = 1
            };
            app.UseHangfireServer(options2);
            */
            // STARTUP JOBS

            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.RequeueOrphanedJobs(), "0 0 * * 0");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.StreamerCheckForUpdates(), "0 0 * * 0");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.TrimLogs(), "0 0 * * 0");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.CheckForStreamerLiveStatus(), "* * * * *");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.RemoveTemp(), "* * * * *");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.CheckStreamFileExists(), "*/5 * * * *");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.RefreshValidation(), "*/5 * * * *");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.CheckForUpdates(), "0 * * * *");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.DatabaseBackup("chatDb"), "0 0 * * 0");
            //RecurringJob.AddOrUpdate<StartupJobs>(item => item.DatabaseBackup("mainDb"), "0 0 * * 0");

            app.UseCors("CorsPolicy");
        }

        public string QuartzDbCheck() {
            string location = $"{SanitizePath()}databases/jobDb.db";

            if (!File.Exists(location)) {
                string query;
                using (WebClient client = new WebClient()) {
                    query = client.DownloadString(
                        "https://raw.githubusercontent.com/vigetious/quartznet/main/database/tables/tables_sqlite.sql");
                }

                using (var connection = new System.Data.SQLite.SQLiteConnection(@"Data Source=" + location)) {
                    connection.Open();
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, connection)) {
                        command.ExecuteScalar();
                    }
                }
            }

            return location;
        }

        public string SanitizePath() {
            string path = ConfigurationManager.Configuration["ContentRootPath"];

            if (!path.EndsWith("/")) {
                return path + "/";
            }

            return path;
        }


        public string AddContentRootPathToDatabase(string contentRootPath) {
            using (var context = new MainDataContext()) {
                var existingContentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath");
                if (existingContentRootPath == null) {
                    context.Add(new Config {
                        key = "contentRootPath",
                        value = contentRootPath
                    });
                    context.SaveChanges();
                    return contentRootPath;
                }

                var newPath = SanitizePath();
                if (newPath != existingContentRootPath.value) {
                    existingContentRootPath.value = newPath;
                    context.SaveChanges();
                    return newPath;
                }

                return existingContentRootPath.value;
            }
        }
    }
}