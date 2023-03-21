using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;

namespace SocialConnect.WebUI.Controllers;

[Authorize]
public class NewsController : Controller
{
    private readonly INewsRepository _newsRepository;
    private readonly IUserRepository _userRepository;

    public NewsController(INewsRepository newsRepository,
                          IUserRepository userRepository)
    {
        _newsRepository = newsRepository;
        _userRepository = userRepository;
    }
    
    public async Task<IActionResult> All()
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        User? user = await _userRepository.FirstOrDefaultAsync(user => user.Id == userId);

        if (user == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        IEnumerable<string> friends = user.Friends.Select(friend => friend.FriendId);
        IEnumerable<string> groups = user.Groups.Where(group => group.IsAgreed)
                                                .Select(group => group.GroupId!);
        IEnumerable<News> news = await _newsRepository.GetNewsFromUsersNGroupsAsync(friends, groups);

        return View(news);
    }
}