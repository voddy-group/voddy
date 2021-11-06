using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Notifications {
    [ApiController]
    [Route("notifications")]
    public class Notifications {
        private NotificationLogic _notificationLogic { get; set; }
        
        public Notifications() {
            _notificationLogic = new NotificationLogic();
        }
        [HttpGet]
        public List<Notification> ListNotifications(Position position) {
            return _notificationLogic.ListNotificationsLogic(position);
        }
    }
}