using SocialConnect.Domain.Enums;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class NewsContent : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public NewsContentType Type { get; set; }
    
    // Relations
    public string NewsId { get; set; } = string.Empty;
    public News News { get; set; } = null!;
}