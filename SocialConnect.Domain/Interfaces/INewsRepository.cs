using SocialConnect.Domain.Entities;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces;

public interface INewsRepository : IRepository<News>
{
    Task<bool> LikeAsync(string userId, string newsId);

    Task<bool> CommentAsync(string userId, string newsId, string content);

    Task<bool> LikeCommentAsync(string userId, string commentId);

    Task<bool> RemoveRangeAsync(IEnumerable<News> news);
}