using System;
using voddy.Databases.Main.Models;

namespace voddy.Controllers {
    public class StreamExtended : Stream {
        public string thumbnailLocation { get; set; }
        public DateTime started_at { get; set; }
    }
}