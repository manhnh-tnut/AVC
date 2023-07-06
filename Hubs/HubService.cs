using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AVC.Hubs
{
    public class HubService : Hub<IHubService>
    {
        public override async Task OnConnectedAsync()
        {
            var ip = Context.GetHttpContext().Connection.RemoteIpAddress.ToString();
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, ip);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var ip = Context.GetHttpContext().Connection.RemoteIpAddress.ToString();
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, ip);
            await base.OnDisconnectedAsync(exception);
        }
    }
}