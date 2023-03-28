using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Infrastructure.Interfaces;
using SocialConnect.WebUI.Extenstions;
using SocialConnect.WebUI.ViewModels;

namespace SocialConnect.WebUI.Controllers;

[Authorize]
public class NewsController : Controller
{
    private readonly INewsRepository _newsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public NewsController(INewsRepository newsRepository,
                          IUserRepository userRepository,
                          IGroupRepository groupRepository,
                          IBlobService blobService,
                          IMapper mapper)
    {
        _newsRepository = newsRepository;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _blobService = blobService;
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
        
        if (!ModelState.IsValid)
        {
            return View(nameof(All));
        }
        
        News news = new() { GroupId = newsVm.GroupId };
        
        List<NewsContent> content = new List<NewsContent> { new NewsContent { Content = newsVm.Description, NewsId = news.Id, Type = NewsContentType.Text }};
        
        if (newsVm.Image is IFormFile file && file.ContentType.Contains("image"))
        {
            string filename = await UploadImageAsync(file);
            NewsContent img = new () { Content = $"news/{filename}", NewsId = news.Id, Type = NewsContentType.Image };
            content.Add(img);
        }

        news.UserId = userId;
        news.Contents = content;
        
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
            return GetUserIdError();
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

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            return GetUserIdError();
        }

        News? news = await _newsRepository.FirstOrDefaultAsync(news => news.Id == id);
        
        if (news == null)
        {
            ErrorVM error = new()
            {
                Title = "News error?!",
                Content = "Unknown news."
            };
            return View("Error", error);
        }

        NewsVM newsVm = new()
        {
            Description = news.Contents.FirstOrDefault(content => content.Type == NewsContentType.Text)?.Content ?? ""
        };
        return View(newsVm);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(NewsVM newsVm, string id)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            return GetUserIdError();
        }
        if (!ModelState.IsValid)
        {
            return View(newsVm);
        }

        News news = new();
        IList<NewsContent> newsContents = new List<NewsContent>();
        newsContents.Add(new () { Type = NewsContentType.Text, Content = newsVm.Description, NewsId = id });
        news.Contents = newsContents;
        await _newsRepository.UpdateAsync(id, news);
        
        return RedirectToAction(nameof(Info), new { id });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        string? userId = User.GetUserId();
        if (userId == null)
        {
            return BadRequest();
        }

        News? news = await _newsRepository.FirstOrDefaultAsync(news => news.Id == id);
        
        if (news == null || news.UserId != userId)
        {
            return BadRequest();
        }

        await Task.WhenAll(news.Contents.Where(content => content.Type != NewsContentType.Text)
                                            .Select(content => DeleteFileAsync(content.Content)));

        await _newsRepository.DeleteAsync(id);

        return Ok();
    }

    private async Task DeleteFileAsync(string filename)
    {
        int indexOfSlash = filename.IndexOf('/') + 1;
        string correctFilename = filename.Substring(indexOfSlash, filename.Length - indexOfSlash);
        await _blobService.DeleteFileAsync(correctFilename);
    }

    private IActionResult GetUserIdError()
    {
        ErrorVM error = new()
        {
            Title = "User error?!",
            Content = "Try re-login."
        };
        return View("Error", error);
    }

    private async Task<string> UploadImageAsync(IFormFile file)
    {
        int indexOfDot = file.ContentType.LastIndexOf("/");
        string postfix = "." + file.ContentType.Substring(indexOfDot + 1, file.ContentType.Length - indexOfDot - 1);
        string filename = Guid.NewGuid() + postfix;
        await _blobService.UploadFileAsync(file.OpenReadStream(), filename);

        return filename;
    }
}