using SocialConnect.Domain.Entities;
using SocialConnect.Shared;
using System.Linq.Expressions;

namespace SocialConnect.Domain.Interfaces
{
    public interface IFriendRepository : IRepository<FriendsCouple>
    {
        Task<bool> AcceptAsync(string userId, string friendId);
        Task<bool> DeclineAsync(string userId, string friendId);
        Task<bool> DeleteAsync(string userId, string friendId);
        Task<IQueryable<User>> GetUserFriendsAsync(string userId);
        Task<IQueryable<User>> GetUserFriendsAsync(string userId, Expression<Func<FriendsCouple, bool>> expression);
    }
}
