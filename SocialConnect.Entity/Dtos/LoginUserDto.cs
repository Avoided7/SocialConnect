using System.ComponentModel.DataAnnotations;

namespace SocialConnect.Entity.Dtos
{
    public class LoginUserDto
    {
        [Required, MinLength(4)]
        public string Username { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
