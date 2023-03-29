using SocialConnect.Domain.Entities;
using SocialConnect.Shared;
using System.Linq.Expressions;

namespace SocialConnect.Domain.Interfaces
{
    public interface IFriendRepository : IRepository<FriendsCouple>
    {
        Task<bool> DeleteAsync(string userId, string friendId);
        Task<IReadOnlyCollection<User>> GetUserFriendsAsync(string userId);
        Task<IReadOnlyCollection<User>> GetUserFriendsAsync(string userId, Expression<Func<FriendsCouple, bool>> expression);
    }
}
