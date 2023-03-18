using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Domain.Entities
{
    public class User : IdentityUser
    {
        public string Lastname { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
