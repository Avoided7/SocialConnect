using SocialConnect.Domain.Enums;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities
{
    public class GroupUser : IEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public GroupUserStatus UserStatus { get; set; }
        public bool IsAgreed { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public string? UserId { get; set; }
        public User? User { get; set; }
        public string? GroupId { get; set; }
        public Group? Group { get; set; }
    }
}
