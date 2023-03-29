using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class News : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime WrittenIn { get; set; } = DateTime.UtcNow;
    public bool IsUserNews => GroupId == null;
    

    // Relations
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public string? GroupId { get; set; }
    public Group? Group { get; set; }
    public virtual IList<Comment> Comments { get; set; } = null!;
    public virtual IList<NewsLike> Likes { get; set; } = null!;
    public virtual IList<NewsContent> Contents { get; set; } = null!;
}