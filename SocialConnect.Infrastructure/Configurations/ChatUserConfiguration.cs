using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities;

namespace SocialConnect.Infrastructure.Configurations;

public class ChatUserConfiguration : IEntityTypeConfiguration<ChatUser>
{
  public void Configure(EntityTypeBuilder<ChatUser> builder)
  {
    builder.HasKey(chatUser => new { chatUser.ChatId, chatUser.UserId });
  }
}