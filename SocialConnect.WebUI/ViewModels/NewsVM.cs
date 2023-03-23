using System.ComponentModel.DataAnnotations;

namespace SocialConnect.WebUI.ViewModels;

public class NewsVM
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;

    public string? GroupId { get; set; }
}