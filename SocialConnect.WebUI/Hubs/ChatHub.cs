using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;

namespace SocialConnect.WebUI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            string? groupName = this.Context.GetHttpContext()?.Request.Query["groupName"];

            if (groupName == null)
            {
                return base.OnDisconnectedAsync(new Exception("Incorrect group"));
            }

            this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);
            
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
