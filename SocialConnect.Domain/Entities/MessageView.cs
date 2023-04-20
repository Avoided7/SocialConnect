namespace SocialConnect.Domain.Entities;

public class MessageView
{
    // Relations
    public string? UserId { get; set; }
    public User? User { get; set; }

    public string? MessageId { get; set; }
    public ChatMessage? Message { get; set; }
}