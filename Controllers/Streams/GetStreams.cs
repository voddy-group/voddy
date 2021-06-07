using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Streams {
    [ApiController]
    [Route("streams")]
    public class GetStreams : ControllerBase {
        [HttpGet]
        [Route("getStreams")]
        public HandleDownloadStreamsLogic.GetStreamsResult GetMultipleStreams(int id) {
            GetStreamLogic getStreamLogic = new GetStreamLogic();
            return getStreamLogic.FetchStreams(id);
        }

        [HttpGet]
        [Route("getStreamsWithFilter")]
        public HandleDownloadStreamsLogic.GetStreamsResult GetStreamsWithFilter(int id) {
            GetStreamLogic getStreamLogic = new GetStreamLogic();
            return getStreamLogic.GetStreamsWithFiltersLogic(id);
        }
    }
}