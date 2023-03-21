using SocialConnect.Domain.Entities;
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
    }
}
