using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace voddy.Controllers {
    public class NotificationHub : Hub {
        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Message", "Conncted");
        }

        public void SendNotification() {
            
        }
    }
}