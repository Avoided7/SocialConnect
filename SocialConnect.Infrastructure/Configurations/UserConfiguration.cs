using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities;

namespace SocialConnect.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder
      .HasOne(user => user.Status)
      .WithOne(status => status.User);
  }
}