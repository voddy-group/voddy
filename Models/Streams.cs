using System;
using voddy.Data;

namespace voddy.Models {
    public class Streams: DataContext.TableBase {
        public int streamId { get; set; }
        public int streamerId { get; set; }
        public int quality { get; set; }
        public string title { get; set; }
        public DateTime createdAt { get; set; }
        public string thumbnailLocation { get; set; }
        public TimeSpan duration { get; set; }
    }
}