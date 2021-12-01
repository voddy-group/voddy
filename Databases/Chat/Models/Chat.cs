using System;
using voddy.Databases.Main;

namespace voddy.Databases.Chat.Models {
    public class Chat: MainDataContext.TableBase {
        public string messageId { get; set; }
        public long streamId { get; set; }
        public bool mod { get; set; }
        public bool subscriber { get; set; }
        public bool turbo { get; set; }
        public string emotes { get; set; }
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