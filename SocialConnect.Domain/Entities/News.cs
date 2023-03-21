using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class News : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime WrittenIn { get; set; } = DateTime.UtcNow;
    public string? GroupId { get; set; }
    public bool IsUserNews => GroupId == null;
    

    // Relations
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public virtual IEnumerable<Comment> Comments { get; set; } = null!;
    public virtual IEnumerable<NewsLike> Likes { get; set; } = null!;
}