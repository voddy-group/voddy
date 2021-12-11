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

        public static Notification CreateNotification(string id, Severity severity, Position position, string description, string url = null) {
            Notification notification = new Notification {
                id = id,
                description = description,
                severity = severity,
                position = position,
                url = url
            };

            if (position == Position.Top) {
                // not single fire notification so save it to the database.
                using (var context = new MainDataContext()) {
                    if (!context.Notifications.Any(item => item.id == notification.id)) {
                        context.Notifications.Add(notification);

                        context.SaveChanges();
                    }
                }
            }

            NotificationHub.Current.Clients.All.SendAsync("createNotification",
                notification);

            return notification;
        }

        public static Task DeleteNotification(string id) {
            using (var context = new MainDataContext()) {
                Notification notification = context.Notifications.FirstOrDefault(notification => notification.id == id);

                if (notification != null) {
                    context.Remove(notification);
                    context.SaveChanges();
                }
            }

            NotificationHub.Current.Clients.All.SendAsync("deleteNotification",
                id);

            return Task.CompletedTask;
        }
    }
}