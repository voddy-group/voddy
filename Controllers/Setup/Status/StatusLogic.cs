using System;
using System.Diagnostics;
using System.Linq;
using voddy.Databases.Main;

namespace voddy.Controllers.Setup.Status {
    public class StatusLogic {
        public StatusReturn GetStatusLogic() {
            StatusReturn statusReturn = new StatusReturn {
                Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime,
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                ContentRootPath = GlobalConfig.GetGlobalConfig("contentRootPath")
            };

            var connection = GlobalConfig.GetGlobalConfig("connectionError");
            if (connection != null) {
                statusReturn.Connection = Boolean.Parse((ReadOnlySpan<char>)connection);
            }

            return statusReturn;
        }
    }

    public class StatusReturn {
        public TimeSpan Uptime { get; set; }
        public string ContentRootPath { get; set; }
        public string Version { get; set; }
        public bool Connection { get; set; }
    }
}