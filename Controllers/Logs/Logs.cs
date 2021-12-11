using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Logs {
    [ApiController]
    [Route("logs")]
    public class Logs : ControllerBase {
        [HttpGet]
        public LogsResponse ListLogs(int? pageOffset, int? pageSize, string? levelFilter) {
            return new LogsLogic().ListLogsLogic(pageOffset, pageSize, levelFilter);
        }
    }
}