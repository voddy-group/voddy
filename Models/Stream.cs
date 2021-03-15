using System;
using voddy.Data;

namespace voddy.Models {
    public class Stream: DataContext.TableBase {
        public int streamId { get; set; }
        public int streamerId { get; set; }
        public int quality { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public DateTime createdAt { get; set; }
        public string thumbnailLocation { get; set; }
        public string downloadLocation { get; set; }
        public TimeSpan duration { get; set; }
        public bool downloading { get; set; }
        public string downloadJobId { get; set; }
    }
}