using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class NewsLike : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    
    // Relations
    public string NewsId { get; set; } = string.Empty;
    public News News { get; set; } = null!;
}