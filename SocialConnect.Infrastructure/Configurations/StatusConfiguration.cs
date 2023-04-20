using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities;

namespace SocialConnect.Infrastructure.Configurations;

public class StatusConfiguration : IEntityTypeConfiguration<Status>
{
  public void Configure(EntityTypeBuilder<Status> builder)
  {
    builder
      .HasOne(status => status.User)
      .WithOne(user => user.Status);
  }
}