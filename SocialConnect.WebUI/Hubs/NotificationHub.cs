using Microsoft.AspNetCore.SignalR;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;

namespace SocialConnect.WebUI.Hubs;

public class NotificationHub : Hub
{
    private readonly IUserRepository _userRepository;
    private static readonly Dictionary<string, int> _users = new Dictionary<string, int>();

    public NotificationHub(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public override async Task OnConnectedAsync()
    {
        string? userId = this.Context.User?.GetUserId();

        if(userId == null)
        {
            return;
        }

        if(_users.ContainsKey(userId))
        {
            _users[userId]++;
        }
        else
        {
            _users.Add(userId, 1);
            await ChangeUserStatusAsync(true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? userId = this.Context.User?.GetUserId();

        if (userId == null)
        {
            return;
        }

        if (_users.ContainsKey(userId))
        {
            if (_users[userId] == 1)
            {
                await ChangeUserStatusAsync(false);
                _users.Remove(userId);
            }
            else
            {
                _users[userId]--;
            }    
        }

        
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