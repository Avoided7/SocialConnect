using System.ComponentModel.DataAnnotations.Schema;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities;

public class Status : IEntity
{
    public string Id { get; set; } = null!;
    public bool IsOnline { get; set; } = false;
    public DateTime LastSeenOnline { get; set; } = DateTime.UtcNow;
    
    // Relations

    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}