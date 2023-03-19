using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SocialConnect.Domain.Entitities.Constants;

namespace SocialConnect.Infrastructure.Data
{
    public static class SeedDB
    {
        public static async void SeedRoles(this IApplicationBuilder builder)
        {
            using (var service = builder.ApplicationServices.CreateScope())
            {
                var role = service.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if(!role.Roles.Any())
                {
                    await role.CreateAsync(new IdentityRole(UserConstant.USER));
                    await role.CreateAsync(new IdentityRole(UserConstant.ADMIN));
                }
            }
        }
    }
}
