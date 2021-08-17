using System;
using System.Diagnostics;
using System.Linq;
using voddy.Databases.Main;

namespace voddy.Controllers.Setup.Status {
    public class StatusLogic {
        public StatusReturn GetStatusLogic() {
            StatusReturn statusReturn = new StatusReturn {
                Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime,
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };

            using (var context = new MainDataContext()) {
                statusReturn.ContentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value;
            }
            
            return statusReturn;
        }
    }
    
    public class StatusReturn {
        public TimeSpan Uptime { get; set; }
        public string ContentRootPath { get; set; }
        public string Version { get; set; }
    }
}