using SocialConnect.Domain.Entities;

namespace SocialConnect.WebUI.ViewModels;

public class NewsWithGroupsVM
{
    public IEnumerable<News> News { get; set; } = null!;
    public IEnumerable<Group> Groups { get; set; } = null!;
}