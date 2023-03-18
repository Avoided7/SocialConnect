using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities;

namespace SocialConnect.Infrastructure.Data
{
    public class SocialDbContext : IdentityDbContext<User>
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        { }
    }
}
