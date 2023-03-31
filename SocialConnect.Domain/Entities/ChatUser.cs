namespace SocialConnect.Domain.Entities;

public class ChatUser
{
    // Relations
    public string? UserId { get; set; }
    public User? User { get; set; }
    
    public string? ChatId { get; set; }
    public Chat? Chat { get; set; }
}