using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Enums;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities
{
    public class User : IEntity
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        // Relations
        public virtual IList<FriendsCouple> Friends { get; set; } = null!;
        public virtual IList<GroupUser> Groups { get; set; } = null!;
        public virtual IList<News> News { get; set; } = null!;
        public virtual IList<ChatUser> Chats { get; set; } = null!;
        public virtual IList<MessageView> MessageViews { get; set; } = null!;

        public string StatusId { get; set; } = string.Empty;
        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; } = null!;
    }
}
