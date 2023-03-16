using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SocialConnect.Infrastructure.Data
{
    public class SocialDbContext : IdentityDbContext
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        { }
    }
}
