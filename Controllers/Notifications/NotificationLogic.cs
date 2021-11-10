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

        public Notification CreateNotification(Severity severity, Position position, string description) {
            Notification notification = new Notification {
                uuid = Guid.NewGuid(),
                description = description,
                severity = severity,
                position = position
            };

            if (position == Position.Top) { // not single fire notification so save it to the database.
                using (var context = new MainDataContext()) {
                    context.Notifications.Add(notification);

                    context.SaveChanges();
                }
            }

            NotificationHub.Current.Clients.All.SendAsync("createNotification",
                notification);
            
            return notification;
        }

        public Task DeleteNotification(Guid guid) {
            using (var context = new MainDataContext()) {
                Notification notification = context.Notifications.FirstOrDefault(notification => notification.uuid == guid);

                if (notification != null) {
                    context.Remove(notification);
                    context.SaveChanges();
                }
            }

            NotificationHub.Current.Clients.All.SendAsync("deleteNotification",
                guid);
            
            return Task.CompletedTask;
        }
    }
}