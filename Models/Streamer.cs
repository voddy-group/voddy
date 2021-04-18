using voddy.Data;

namespace voddy.Models {
    public class Streamer: DataContext.TableBase {
        public string streamerId { get; set; }
        public string displayName { get; set; }
        public string username { get; set; }
        public string thumbnailLocation { get; set; }
        public string thumbnailETag { get; set; }
        public bool isLive { get; set; }
        public bool getLive { get; set; }
        public string quality { get; set; }
    }
}