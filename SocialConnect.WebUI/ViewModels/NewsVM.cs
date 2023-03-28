using System.ComponentModel.DataAnnotations;

namespace SocialConnect.WebUI.ViewModels;

public class NewsVM
{
    [Required]
    public string Description { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public string? GroupId { get; set; }
}