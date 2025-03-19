using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TaskManager.API.Hubs
{
    public class TaskHub : Hub
    {
        public async Task SendTaskUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveTaskUpdate", message);
        }
    }
}