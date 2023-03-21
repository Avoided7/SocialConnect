using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class CommentLike : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    
    // Relations
    public string CommentId { get; set; } = string.Empty;
    public Comment Comment { get; set; } = null!;
}