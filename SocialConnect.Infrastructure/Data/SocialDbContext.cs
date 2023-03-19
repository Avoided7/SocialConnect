using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Entitities;

namespace SocialConnect.Infrastructure.Data
{
    public class SocialDbContext : IdentityDbContext<User>
    {
        public DbSet<FriendsCouple> Friends { get; set; } = null!;
        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        { }

    }
}
