using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class Comment : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = null!;
    public DateTime WrittenIn { get; set; } = DateTime.UtcNow;
    
    public int LikesCount => Likes?.Count() ?? 0;
    
    // Relations
    public string NewsId { get; set; } = string.Empty;
    public News News { get; set; } = null!;
    public string? UserId { get; set; } = null!;
    public User? User { get; set; } = null!;
    public IList<CommentLike> Likes { get; set; } = null!;
}