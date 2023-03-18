using System.ComponentModel.DataAnnotations;

namespace SocialConnect.WebUI.ViewModels
{
    public class LoginVM
    {
        [Required, MinLength(4), DataType(DataType.Text)]
        public string Username { get; set; } = string.Empty;
        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
