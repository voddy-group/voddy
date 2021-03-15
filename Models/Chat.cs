using voddy.Data;

namespace voddy.Models {
    public class Chat: DataContext.TableBase {
        public int streamId { get; set; }
        public string comment { get; set; }
        public bool downloading { get; set; }
        public string downloadJobId { get; set; }
    }
}