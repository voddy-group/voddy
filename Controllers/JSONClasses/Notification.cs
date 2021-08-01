using Microsoft.AspNetCore.SignalR;

namespace voddy.Controllers.Structures {
    public class Notification {
        public string name { get; set; }
        public string level { get; set; }
        public string method { get; set; }
        public string message { get; set; }
    }
}