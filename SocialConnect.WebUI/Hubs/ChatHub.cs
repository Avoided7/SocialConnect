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
        private readonly IChatRepository _chatRepository;

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }
        
        public async Task Send(string groupName, string message)
        {
            message = message.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            string username = this.Context.User?.Identity?.Name ?? "Unknown";
            string? userId = this.Context.User?.GetUserId();

            if (userId == null)
            {
                return;
            }

            await _chatRepository.SendMessageByGroupNameAsync(groupName, userId, message);
            
            /*await this.Clients.Caller.SendAsync("Receive", username, message);
            await this.Clients.User(userId).SendAsync("Receive", username, message);*/

            await this.Clients.Group(groupName).SendAsync("Receive", username, message);
        }
        
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
