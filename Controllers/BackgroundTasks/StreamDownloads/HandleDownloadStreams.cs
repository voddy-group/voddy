using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using voddy.Controllers.Structures;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers {
    [ApiController]
    [Route("backgroundTask")]
    public class HandleDownloadStreams : ControllerBase {
        private readonly ILogger<HandleDownloadStreams> _logger;
        private IBackgroundJobClient _backgroundJobClient;
        private IWebHostEnvironment _environment;
        private readonly IHubContext<NotificationHub> _hubContext;

        public HandleDownloadStreams(ILogger<HandleDownloadStreams> logger, IBackgroundJobClient backgroundJobClient,
            IWebHostEnvironment environment, IHubContext<NotificationHub> hubContext) {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _environment = environment;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("downloadStreams")]
        public IActionResult DownloadStreams([FromBody] HandleDownloadStreamsLogic.GetStreamsResult streams, int id) {
            foreach (var stream in streams.data) {
                if (!stream.alreadyAdded) {
                    HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
                    handleDownloadStreamsLogic.PrepareDownload(stream, false);
                }
            }

            return Ok();
        }

        [HttpGet]
        [Route("test")]
        public IActionResult FourHEad() {
            StartupJobs startupJobs = new StartupJobs();
            startupJobs.CheckForStreamerLiveStatus();
            return Ok();
        }

        [HttpPost]
        [Route("downloadStream")]
        public IActionResult DownloadSingleStream([FromBody] HandleDownloadStreamsLogic.Data stream) {
            using (var context = new DataContext()) {
                HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
                if (handleDownloadStreamsLogic.PrepareDownload(stream, false)) {
                    return Ok();
                }
            }

            return Conflict("Already exists.");
        }
    }
}