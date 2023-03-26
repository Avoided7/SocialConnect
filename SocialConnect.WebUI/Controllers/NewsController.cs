using System.Collections;
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
            ErrorVM error = new()
            {
                Title = "User error?!",
                Content = "Try re-login."
            };
            return View("Error", error);
        }

        User? user = await _userRepository.FirstOrDefaultAsync(user => user.Id == userId);

        if (user == null)
        {
            ErrorVM error = new()
            {
                Title = "User error?!",
                Content = "Try re-login."
            };
            return View("Error", error);
        }

        IEnumerable<string> friends = user.Friends.Select(friend => friend.FriendId)
                                                  .Concat(new [] { userId });
        
        IEnumerable<string> groups = user.Groups.Where(group => group.IsAgreed)
                                                .Select(group => group.GroupId!);
        
        IEnumerable<News> news = await _newsRepository.GetNewsFromUsersNGroupsAsync(friends, groups);

        return View(news);
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
            ErrorVM error = new()
            {
                Title = "User error?!",
                Content = "Try re-login."
            };
            return View("Error", error);
        }
        News news = _mapper.Map<News>(newsVm);
        news.UserId = userId;
        
        News? createdNews = await _newsRepository.CreateAsync(news);
        if (createdNews == null)
        {
            ErrorVM error = new()
            {
                Title = "Creating error?!",
                Content = "Try create later."
            };
            return View("Error", error);
        }

        return Redirect(Request.Headers["Referer"]);
    }

    [HttpGet]
    public async Task<IActionResult> Info(string id)
    {
        News? news = await _newsRepository.FirstOrDefaultAsync(news => news.Id == id);
        if (news == null)
        {
            ErrorVM error = new()
            {
                Title = "User error?!",
                Content = "Try re-login."
            };
            return View("Error", error);
        }
        return View(news);
    }

    [HttpGet]
    public async Task<IActionResult> Like(string id)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            ErrorVM error = new()
            {
                Title = "User error?!",
                Content = "Try re-login."
            };
            return View("Error", error);
        }

        bool isLiked = await _newsRepository.LikeAsync(userId, id);
        if (!isLiked)
        {
            ErrorVM error = new()
            {
                Title = "Error?!",
                Content = "Please, try later."
            };
            return View("Error", error);
        }

        return Redirect(Request.Headers["Referer"]);
    }

    [HttpPost]
    public async Task<IActionResult> Comment(string id, string content)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            ErrorVM error = new()
            {
                Title = "User error?!",
                Content = "Try re-login."
            };
            return View("Error", error);
        }

        bool isCommented = await _newsRepository.CommentAsync(userId, id, content);

        if (!isCommented)
        {
            ErrorVM error = new()
            {
                Title = "Error?!",
                Content = "Please, try later."
            };
            return View("Error", error);
        }

        return Redirect(Request.Headers["Referer"]);
    }

    [HttpGet]
    public async Task<IActionResult> LikeComment(string id)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            ErrorVM error = new()
            {
                Title = "User error?!",
                Content = "Try re-login."
            };
            return View("Error", error);
        }

        bool isLiked = await _newsRepository.LikeCommentAsync(userId, id);

        if (!isLiked)
        {
            ErrorVM error = new()
            {
                Title = "Error?!",
                Content = "Please, try later."
            };
            return View("Error", error);
        }

        return Redirect(Request.Headers["Referer"]);
    }
}