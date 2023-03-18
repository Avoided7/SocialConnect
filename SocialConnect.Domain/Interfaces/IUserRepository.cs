using SocialConnect.Domain.Entities;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> LoginAsync(string login, string password);
        Task<bool> RegisterAsync(User user, string password);
        Task<User?> FindByIdAsync(string id);
        Task<User?> FindByUsernameAsync(string username);
        Task<bool> ConfirmAsync(string userId, string token);
        Task<bool> LogoutAsync();
    }
}
