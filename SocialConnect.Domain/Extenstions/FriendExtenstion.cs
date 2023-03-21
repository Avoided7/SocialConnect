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
}