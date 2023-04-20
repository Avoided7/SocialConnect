using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class Chat : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // For SignalR hub
    public string GroupName { get; set; } = Guid.NewGuid().ToString();
    public bool IsCoupleChat { get; set; }
    
    /* Here can be more settings like background, chat-name, etc. */
    
    // Relations
    public virtual IList<ChatMessage> Messages { get; set; } = null!;
    public virtual IList<ChatUser> Users { get; set; } = null!;
}