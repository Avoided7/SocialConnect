using SocialConnect.Domain.Entities;

namespace SocialConnect.Domain.Interfaces
{
    public interface IAccountService
    {
        Task<bool> LoginAsync(string login, string password);
        Task<bool> RegisterAsync(User user, string password);
        Task<User?> FindByIdAsync(string id);
        Task<User?> FindByUsernameAsync(string username);
        Task<bool> DeleteAsync(string id);
        Task<bool> ConfirmAsync(string userId, string token);
        Task<bool> LogoutAsync();
    }
}
