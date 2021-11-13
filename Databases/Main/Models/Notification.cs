using System;
using System.ComponentModel.DataAnnotations;

namespace voddy.Databases.Main.Models {
    public class Notification {
        [Key]
        public Guid uuid { get; set; }
        public Severity severity { get; set; }
        public Position position { get; set; }
        public string description { get; set; }
        public string url { get; set; }
    }

    public enum Severity {
        Info,
        Warning,
        Error
    }

    public enum Position {
        Top,
        General
    }
}