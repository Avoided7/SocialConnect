using SocialConnect.Domain.Entities;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<bool> JoinUserAsync(string groupId, string userId);
        Task<bool> LeftUserAsync(string groupId, string userId);
        Task<bool> PromoteUserAsync(GroupUser groupUser);
    }
}
