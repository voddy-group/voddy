using voddy.Data;

namespace voddy.Models {
    public class Streamer: DataContext.TableBase {
        public string streamId { get; set; }
        public string displayName { get; set; }
        public string username { get; set; }
        public string thumbnailLocation { get; set; }
        public bool isLive { get; set; }
    }
}