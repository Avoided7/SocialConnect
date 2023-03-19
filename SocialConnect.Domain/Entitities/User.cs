using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Entitities;
using SocialConnect.Domain.Enums;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities
{
    public class User : IdentityUser, IEntity
    {
        public string Lastname { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        // Relations
        public virtual IEnumerable<FriendsCouple> Friends { get; set; } = null!;
    }
}
