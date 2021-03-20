using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using voddy.Data;
using static voddy.Controllers.HandleDownloadStreams;


namespace voddy {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }
        
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllersWithViews();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddMvc();
            services.AddDbContext<DataContext>();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });

            // hangfire
            services.AddHangfire(c => c.UseMemoryStorage());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                Console.WriteLine("Web root path: " + env.ContentRootPath);
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions{
                FileProvider = new PhysicalFileProvider(env.ContentRootPath),
                RequestPath = "/voddy"
            });
            app.UseSpaStaticFiles();

            app.UseSession();
            

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapHangfireDashboard();
            });

            app.UseSpa(spa => {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment()) {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
            
            //hangfire
            var options = new BackgroundJobServerOptions();
            using (var context = new DataContext()) {
                var workerCountValue = context.Configs.FirstOrDefault(item => item.key == "workerCount");

                if (workerCountValue != null) {
                    options.WorkerCount = Int32.Parse(workerCountValue.value);
                }
            }

            app.UseHangfireServer(options);
            BackgroundJob.Enqueue(() => CheckForInterruptedDownloads());
        }
        
        public async Task CheckForInterruptedDownloads() {
            Console.WriteLine("Checking for interrupted stream/chat downloads...");
            using (var context = new DataContext()) {
                foreach (var chat in context.Chats.AsList()) {
                    if (chat.downloading) {
                        Console.WriteLine($"Downloading {chat.streamId} chat.");
                        await DownloadChat(chat.streamId, CancellationToken.None);
                    }
                }

                foreach (var stream in context.Streams.AsList()) {
                    if (stream.downloading) {
                        Console.WriteLine($"Downloading {stream.streamId} VOD.");
                        await DownloadStream(stream.streamId, stream.downloadLocation, stream.url, CancellationToken.None);
                    }
                }
            }
            Console.WriteLine("Done checking!");
        }
    }
}