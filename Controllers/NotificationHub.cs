using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace voddy.Controllers {
    public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public static IHubContext<NotificationHub> Current { get; set; }
    }
}