using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities
{
    public class FriendsCouple : IEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string FriendId { get; set; } = string.Empty;
        public bool IsAgreed { get; set; } = false;

        // Relations
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }
}
