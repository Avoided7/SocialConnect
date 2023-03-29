using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<bool> AddUserToGroupAsync(string groupId, GroupUser groupUser);
        Task<bool> RemoveUserFromGroupAsync(string groupId, string userId);
        Task<GroupUser?> UpdateGroupUserAsync(string groupId, string userId, GroupUser user);
        Task<GroupUserStatus?> GetUserStatusAsync(string groupId, string userId);
    }
}
