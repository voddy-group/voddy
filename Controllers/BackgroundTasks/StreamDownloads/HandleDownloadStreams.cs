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
using voddy.Databases.Main;

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
        public IActionResult DownloadStreams(int id) {
            HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
            handleDownloadStreamsLogic.DownloadAllStreams(id);

            return Ok();
        }

        [HttpPost]
        [Route("downloadStream")]
        public IActionResult DownloadSingleStream(long streamId) {
            using (var context = new MainDataContext()) {
                HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
                if (handleDownloadStreamsLogic.DownloadSingleStream(streamId, null)) {
                    return Ok();
                }
            }

            return Conflict("Already exists.");
        }
    }
}