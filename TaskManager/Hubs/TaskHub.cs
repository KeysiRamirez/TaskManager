using Microsoft.AspNetCore.SignalR;

namespace TaskManager.API.Hubs
{
    public class TaskHub : Hub
    {
        public async Task SendTaskUpdate(string user, string message)
           => await Clients.All.SendAsync("ReceiveTaskUpdate", user, message);
        
    }
}