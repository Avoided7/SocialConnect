using System.ComponentModel.DataAnnotations;

namespace SocialConnect.WebUI.ViewModels
{
    public class GroupVM
    {
        [Required, MinLength(4), MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required, MinLength(10), MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
}
