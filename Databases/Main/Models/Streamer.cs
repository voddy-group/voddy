namespace voddy.Databases.Main.Models {
    public class Streamer: MainDataContext.TableBase {
        public int streamerId { get; set; }
        public string displayName { get; set; }
        public string username { get; set; }
        public string description { get; set; }
        public int? viewCount { get; set; }
        public string thumbnailLocation { get; set; }
        public string thumbnailETag { get; set; }
        public bool? isLive { get; set; }
        public bool? getLive { get; set; }
        public string quality { get; set; }
    }
}