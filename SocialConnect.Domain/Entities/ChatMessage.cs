using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class ChatMessage : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime WrittenAt { get; set; } = DateTime.UtcNow;

    // Relations
    public string ChatId { get; set; } = string.Empty;
    public Chat Chat { get; set; } = null!;

    public virtual IList<MessageView> Views { get; set; } = null!;
}