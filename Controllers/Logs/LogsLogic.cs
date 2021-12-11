using System;
using System.Collections.Generic;
using System.Linq;
using voddy.Databases.Logs;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Logs {
    public class LogsLogic {
        public LogsResponse ListLogsLogic(int? pageOffset, int? pageSize, string? levelFilter) {
            using (var context = new LogDataContext()) {
                
                var orderedLogs = context.Logs.OrderByDescending(log => log.logged);
                int count = context.Logs.Count();
                LogsResponse logsResponse = new LogsResponse {
                    count = count
                };
                if (pageOffset != null && pageSize != null) {
                    if (count > pageSize + pageOffset) {
                        logsResponse.logs = levelFilter != null ? orderedLogs.Where(item => levelFilter != null && item.level == levelFilter).Skip(pageOffset.Value).Take(pageSize.Value).ToList() : orderedLogs.Skip(pageOffset.Value).Take(pageSize.Value).ToList();

                        return logsResponse;
                    } else {
                        logsResponse.logs = new List<Log>();
                        return logsResponse;
                    }
                } else {
                    logsResponse.logs = levelFilter != null ? orderedLogs.Where(item => levelFilter != null && item.level == levelFilter).ToList() : orderedLogs.ToList();
                    return logsResponse;
                }
            }
        }
    }

    public class LogsResponse {
        public List<Log> logs { get; set; }
        public int count { get; set; }
    }
}