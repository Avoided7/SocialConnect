using SocialConnect.Entity.Enums;
using System.ComponentModel.DataAnnotations;

namespace SocialConnect.Entity.Dtos
{
    public class RegisterUserDto
    {
        [Required, MinLength(4)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Firstname { get; set; } = string.Empty;
        [Required]
        public string Lastname { get; set; } = string.Empty;
        [Required]
        public Gender Gender { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), Compare("Password")]
        public string PasswordConfirm { get; set; } = string.Empty;
    }
}
