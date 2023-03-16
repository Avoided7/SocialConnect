using SocialConnect.Entity.Models;
using SocialConnect.Entity.Dtos;

namespace SocialConnect.Domain.Extenstions
{
    internal static class UserExtenstion
    {
        public static UserDto ConvertToDto(this User user)
        {
            return new UserDto
            {
                Username = user.UserName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth
            };
        }
    }
}
