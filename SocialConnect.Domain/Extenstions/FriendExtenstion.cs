using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.Domain.Extenstions;

public static class FriendExtenstion
{
    public static async Task<int> GetFriendRequestsCountAsync(this IFriendRepository friendRepository, string userId)
    {
        IEnumerable<FriendsCouple> friendRequests = await friendRepository
            .GetAsync(friends => !friends.IsAgreed && friends.FriendId == userId);
        int count = friendRequests.Count();

        return count;
    }

    public static async Task<bool> AcceptAsync(this IFriendRepository friendRepository, string userId, string friendId)
    {
        FriendsCouple? friend = await friendRepository.FirstOrDefaultAsync(friendCouple => friendCouple.UserId == userId &&
                                                                                           friendCouple.FriendId == friendId);
        if (friend == null || friend.IsAgreed)
        {
            return false;
        }
        friend.IsAgreed = true;

        await friendRepository.UpdateAsync(friend.Id, friend);

        FriendsCouple friendsCouple = new()
        {
            UserId = friend.FriendId,
            FriendId = friend.UserId,
            IsAgreed = true
        };

        
        await friendRepository.CreateAsync(friendsCouple);

        return true;
    }

    public static async Task<bool> DeclineAsync(this IFriendRepository friendRepository, string userId, string friendId)
    {
        FriendsCouple? friend = await friendRepository.FirstOrDefaultAsync(friendCouple => friendCouple.UserId == userId &&
                                                                                           friendCouple.FriendId == friendId);
        if (friend == null || friend.IsAgreed)
        {
            if (friend != null)
            {
                return false;
            }

            friend = await friendRepository.FirstOrDefaultAsync(friendCouple => friendCouple.UserId == friendId &&
                                                                                friendCouple.FriendId == userId);
            if (friend == null)
            {
                return false;
            }
        }
        await friendRepository.DeleteAsync(friend.Id);

        return true;
    }
}