using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Notifications {
    public class NotificationLogic {
        public List<Notification> ListNotificationsLogic(Position position) {
            using (var context = new MainDataContext()) {
                return context.Notifications.Where(notification => notification.position == position).ToList();
            }
        }

        public Task CreateNotificationLogic(Severity severity, Position position, string description) {
            Notification notification = new Notification {
                uuid = Guid.NewGuid(),
                description = description,
                severity = severity,
                position = position
            };

            NotificationHub.Current.Clients.All.SendAsync("createNotification",
                notification);
            
            return Task.CompletedTask;
        }

        public Task DeleteNotificationLogic(Guid guid) {
            using (var context = new MainDataContext()) {
                Notification notification = context.Notifications.FirstOrDefault(notification => notification.uuid == guid);

                if (notification != null) {
                    context.Remove((object)notification);
                    context.SaveChanges();
                }
            }

            NotificationHub.Current.Clients.All.SendAsync("deleteNotification",
                guid);
            
            return Task.CompletedTask;
        }
    }
}