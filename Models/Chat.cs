using System;
using voddy.Data;

namespace voddy.Models {
    public class Chat: DataContext.TableBase {
        public long streamId { get; set; }
        public string body { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public DateTime sentAt { get; set; }
        public double offsetSeconds { get; set; }
        public string userBadges { get; set; }
        public string userColour { get; set; }
        public string downloadJobId { get; set; }
    }
}