using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities;

namespace SocialConnect.Infrastructure.Configurations;

public class MessageViewConfiguration : IEntityTypeConfiguration<MessageView>
{
  public void Configure(EntityTypeBuilder<MessageView> builder)
  {
    builder.HasKey(messageView => new { messageView.MessageId, messageView.UserId });
  }
}