using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;
using SocialConnect.WebUI.ViewModels;

namespace SocialConnect.WebUI.Controllers;

[Authorize]
public class NewsController : Controller
{
    private readonly INewsRepository _newsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;

    public NewsController(INewsRepository newsRepository,
                          IUserRepository userRepository,
                          IGroupRepository groupRepository,
                          IMapper mapper)
    {
        _newsRepository = newsRepository;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _mapper = mapper;
    }
    [HttpGet]
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

        IEnumerable<string> friends = user.Friends.Select(friend => friend.FriendId).Concat(new [] { userId });
        IEnumerable<string> groups = user.Groups.Where(group => group.IsAgreed)
                                                .Select(group => group.GroupId!);
        
        IEnumerable<News> news = await _newsRepository.GetNewsFromUsersNGroupsAsync(friends, groups);
        IEnumerable<string> groupsWithNews = news.Where(news => news.GroupId != null).Select(news => news.GroupId).Distinct();
        IEnumerable<Group> allGroups = await _groupRepository.GetAsync(group => groupsWithNews.Contains(group.Id));

        NewsWithGroupsVM newsWithGroups = new()
        {
            News = news,
            Groups = allGroups
        };
        
        return View(newsWithGroups);
    }

    [HttpPost]
    public async Task<IActionResult> Create(NewsVM newsVm)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(All));
        }

        string? userId = User.GetUserId();
        if (userId == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }
        News news = _mapper.Map<News>(newsVm);
        news.UserId = userId;
        
        News? createdNews = await _newsRepository.CreateAsync(news);
        if (createdNews == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        return Redirect(Request.Headers["Referer"]);
    }

    [HttpGet]
    public async Task<IActionResult> Info(string id)
    {
        News? news = await _newsRepository.FirstOrDefaultAsync(news => news.Id == id);
        if (news == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        IEnumerable<User> users = await Task.WhenAll(news.Comments.Select(async comment => (await _userRepository.FindByIdAsync(comment.UserId))!));
        if (!news.IsUserNews)
        {
            ViewData["GroupName"] = (await _groupRepository.FirstOrDefaultAsync(group => group.Id == news.GroupId))?.Name;
        }
        ViewData["Comments"] = users;
        return View(news);
    }

    [HttpGet]
    public async Task<IActionResult> Like(string id)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        bool isLiked = await _newsRepository.LikeAsync(userId, id);
        if (!isLiked)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        return Redirect(Request.Headers["Referer"]);
    }

    [HttpPost]
    public async Task<IActionResult> Comment(string id, string content)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        bool isCommented = await _newsRepository.CommentAsync(userId, id, content);

        if (!isCommented)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        return Redirect(Request.Headers["Referer"]);
    }

    [HttpGet]
    public async Task<IActionResult> LikeComment(string id)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        bool isLiked = await _newsRepository.LikeCommentAsync(userId, id);

        if (!isLiked)
        {
            // TODO: Replace 'Bad Request' with error page/message.
            return BadRequest();
        }

        return Redirect(Request.Headers["Referer"]);
    }
}