using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities;

namespace SocialConnect.Infrastructure.Data
{
    public class SocialDbContext : IdentityDbContext
    {
        public DbSet<FriendsCouple> Friends { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<GroupUser> GroupUsers { get; set; } = null!;
        public DbSet<News> News { get; set; } = null!;
        public DbSet<Comment> Comments  { get; set; } = null!;
        public DbSet<CommentLike> CommentLikes { get; set; } = null!;
        public DbSet<NewsLike> NewsLikes { get; set; } = null!;
        public DbSet<User> SocialUsers { get; set; } = null!;
        public DbSet<NewsContent> NewsContents { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<ChatUser> ChatsUsers { get; set; } = null!;
        public DbSet<ChatMessage> ChatsMessages { get; set; } = null!;
        public DbSet<MessageView> MessagesViews { get; set; } = null!;

        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChatUser>().HasKey(chatUser => new { chatUser.ChatId, chatUser.UserId });
            builder.Entity<MessageView>().HasKey(messageView => new { messageView.MessageId, messageView.UserId });
            
            base.OnModelCreating(builder);
        }
    }
}
