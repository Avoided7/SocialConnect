using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<bool> JoinUserAsync(string groupId, string userId);
        Task<bool> KickUserAsync(string currentUserId, string userId, string groupId);
        Task<bool> LeftUserAsync(string groupId, string userId);
        Task<bool> PromoteUserAsync(string currentUserId, string userId, string groupId, GroupUserStatus newStatus);
        Task<bool> AcceptUserAsync(string currenUserId, string userId, string groupId);
        Task<bool> DeclineRequestAsync(string userId, string groupId);
        Task<bool> DeclineRequestAsync(string currentUserId, string userId, string groupId);
    }
}
