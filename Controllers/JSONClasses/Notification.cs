using System;
using Microsoft.AspNetCore.SignalR;

namespace voddy.Controllers.Structures {
    public class NotificationLogic {
        public static string SendNotification(string level, string message) {
            string notificationUuid = Guid.NewGuid().ToString();
            NotificationHub.Current.Clients.All.SendAsync("createNotification", new Notification {
                id = notificationUuid,
                level = level,
                message = message
            });

            return notificationUuid;
        }

        public static void DeleteNotification(string uuid) {
            NotificationHub.Current.Clients.All.SendAsync("deleteNotification", new Notification {
                id = uuid
            });
        }
    }

    public class Notification {
        public string id { get; set; }
        public string level { get; set; }
        public string message { get; set; }
    }
}