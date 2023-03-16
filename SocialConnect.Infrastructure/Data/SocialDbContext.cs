using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Entity.Models;

namespace SocialConnect.Infrastructure.Data
{
    public class SocialDbContext : IdentityDbContext<User>
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        { }
    }
}
