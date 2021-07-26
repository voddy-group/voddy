using System;

namespace voddy.Databases.Main.Models {
    public class Stream: MainDataContext.TableBase {
        public long streamId { get; set; }
        public long vodId { get; set; }
        public int streamerId { get; set; }
        public int quality { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public DateTime createdAt { get; set; }
        public string thumbnailLocation { get; set; }
        public string downloadLocation { get; set; }
        public int duration { get; set; }
        public bool downloading { get; set; }
        public string downloadJobId { get; set; }
        public bool chatDownloading { get; set; }
        public string chatDownloadJobId { get; set; }
        public long size { get; set; }
    }
}