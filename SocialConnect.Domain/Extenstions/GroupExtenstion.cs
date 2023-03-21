using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.Domain.Extenstions
{
    public static class GroupExtenstion
    {
        public static async Task<bool> IsFounderAsync(this IGroupRepository groupRepository, string groupId, string userId)
        {
            Group? group = await groupRepository.FirstOrDefaultAsync(group => group.Id == groupId);
            if(group == null)
            {
                return false;
            }
            GroupUser? user = group.Users.FirstOrDefault(user => user.UserId == userId);
            if(user == null)
            {
                return false;
            }

            return user.UserStatus == Enums.GroupUserStatus.Founder;
        }
        public static async Task<int> GetGroupRequestsCountAsync(this IGroupRepository groupRepository, string userId)
        {
            IEnumerable<Group> groupRequests = await groupRepository.GetAsync(group => group.Users.Any(groupUser => groupUser.UserId == userId) && group.Users.First(groupUser => groupUser.UserId == userId).UserStatus != GroupUserStatus.User);
            int count = groupRequests.Sum(group => group.Users.Count(user => user.IsAgreed == false));

            return count;
        }
    }
}
