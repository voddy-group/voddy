using System;

namespace voddy.Databases.Main.Models {
    public class JobTriggerExecution: MainDataContext.TableBase {
        public string Name { get; set; }
        public string Group { get; set; }
        public string Scheduler { get; set; }
        public DateTime LastFireDateTime { get; set; }
    }
}