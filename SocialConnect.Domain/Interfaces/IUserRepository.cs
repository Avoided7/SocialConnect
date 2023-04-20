using SocialConnect.Domain.Entities;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> FindByIdAsync(string id);
        Task<User?> FindByUsernameAsync(string username);
        Task<bool> UpdateUserStatusAsync(string userId, Status status);
        Task<Status?> GetUserStatusAsync(string userId);
    }
}
