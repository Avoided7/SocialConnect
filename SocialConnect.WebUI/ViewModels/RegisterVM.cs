using SocialConnect.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SocialConnect.WebUI.ViewModels
{
    public class RegisterVM
    {
        [Required, DataType(DataType.Text)]
        public string Firstname { get; set; } = string.Empty;
        [Required, DataType(DataType.Text)]
        public string Lastname { get; set; } = string.Empty;
        [Required, MinLength(4)]
        public string Username { get; set; } = string.Empty;
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public Gender Gender { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, MinLength(8), Compare("Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
