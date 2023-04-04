using Microsoft.AspNetCore.SignalR;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;

namespace SocialConnect.WebUI.Hubs;

public class NotificationHub : Hub
{
    private readonly IUserRepository _userRepository;

    public NotificationHub(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public override async Task OnConnectedAsync()
    {
        await ChangeUserStatusAsync(true);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await ChangeUserStatusAsync(false);
        
        await base.OnDisconnectedAsync(exception);
    }

    private async Task ChangeUserStatusAsync(bool online)
    {
        string? userId = this.Context.User?.GetUserId();
        if (userId == null)
        {
            return;
        }

        await _userRepository.ChangeStatusAsync(userId, online);

        await this.Clients.All.SendAsync("ChangeStatus", userId, online);
    }
}