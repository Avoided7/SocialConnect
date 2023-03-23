using SocialConnect.Domain.Entities;

namespace SocialConnect.WebUI.ViewModels;

public class GroupWithNewsVM
{
    public Group Group { get; set; } = null!;
    public IEnumerable<News> News { get; set; } = null!;
}