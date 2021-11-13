using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using voddy.Controllers.BackgroundTasks.RecurringJobs;
using voddy.Controllers.BackgroundTasks.RecurringJobs.StartupJobs;
using voddy.Controllers.BackgroundTasks.LiveStreamDownloads.LiveStreamDownloadJobs;
using voddy.Databases.Main;

namespace voddy.Controllers.BackgroundTasks.StreamDownloads {
    [ApiController]
    [Route("backgroundTask")]
    public class HandleDownloadStreams : ControllerBase {
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
                if (handleDownloadStreamsLogic.PrepareDownload(StreamHelpers.GetStreamDetails(streamId))) {
                    return Ok();
                }
            }

            return Conflict("Already exists.");
        }
    }
}