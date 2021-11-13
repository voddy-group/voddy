using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Notifications {
    [ApiController]
    [Route("notifications")]
    public class Notifications {
        [HttpGet]
        public List<Notification> ListNotifications(Position position) {
            NotificationLogic notificationLogic = new NotificationLogic();
            return notificationLogic.ListNotificationsLogic(position);
        }

        [HttpDelete]
        public Task DeleteNotification(Guid uuid) {
            return NotificationLogic.DeleteNotification(uuid);
        }
    }
}