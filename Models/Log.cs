using System;
using voddy.Data;

namespace voddy.Models {
    public class Log: DataContext.TableBase {
        public string application { get; set; }
        public string logged { get; set; }
        public string level { get; set; }
        public string message { get; set; }
        public string logger { get; set; }
        public string callsite { get; set; }
        public string exception { get; set; }
    }
}