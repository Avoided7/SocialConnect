using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.Domain.Extenstions;

public static class UserExtenstion
{
    public static async Task<bool> ChangeStatusAsync(this IUserRepository userRepository,
        string userId,
        bool online = false)
    {
        Status status = new ()
        {
            IsOnline = online,
            LastSeenOnline = DateTime.UtcNow
        };

        return await userRepository.UpdateUserStatusAsync(userId, status);
    }
}