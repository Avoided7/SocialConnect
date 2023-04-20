using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;
using SocialConnect.WebUI.ViewModels;

namespace SocialConnect.WebUI.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly IGroupRepository _groupRepository;
        private readonly INewsRepository _newsRepository;
        private readonly IMapper _mapper;

        public GroupsController(IGroupRepository groupRepository,
                                INewsRepository newsRepository,
                                IMapper mapper)
        {
            this._groupRepository = groupRepository;
            _newsRepository = newsRepository;
            this._mapper = mapper;
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
            IReadOnlyCollection<Group> groups = await _groupRepository.GetAsync(group => group.Users.Any(u => u.UserId == userId));
            return View(groups);
        }
        [HttpGet]
        public async Task<IActionResult> Join(string groupId)
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

            bool isJoined = await _groupRepository.JoinUserAsync(groupId: groupId, userId: userId);

            if(!isJoined)
            {
                ErrorVM error = new()
                {
                    Title = "Join error?!",
                    Content = "Try join later."
                };
                return View("Error", error);
            }

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet]
        public async Task<IActionResult> Left(string groupId)
        {
            string? userId = User.GetUserId();
            if(userId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }

            bool isLeft = await _groupRepository.LeftUserAsync(groupId: groupId, userId: userId);

            if (!isLeft)
            {
                ErrorVM error = new()
                {
                    Title = "Left error?!",
                    Content = "Please, try later."
                };
                return View("Error", error);
            }

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet("{controller}/{action}/{groupName}")]
        public async Task<IActionResult> Info(string groupName)
        {
            Group? group = await _groupRepository.FirstOrDefaultAsync(group => group.Name == groupName);

            if (group == null)
            {
                ErrorVM error = new()
                {
                    Title = "Not found?!",
                    Content = "Group not found! Please check group name."
                };
                return View("Error", error);
            }

            return View(group);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(GroupVM groupVm)
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
                return View(groupVm);
            }

            Group group = _mapper.Map<Group>(groupVm);
            Group? createdGroup = await _groupRepository.CreateAsync(group);
            if (createdGroup == null)
            {
                ErrorVM error = new()
                {
                    Title = "Create group error?!",
                    Content = "Try create group later."
                };
                return View("Error", error);
            }

            await _groupRepository.JoinUserAsync(createdGroup.Id, userId);

            return RedirectToAction(nameof(Info), new { groupName = group.Name });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string groupId)
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
            bool isFounder = await _groupRepository.IsFounderAsync(groupId, userId);

            if (!isFounder)
            {
                return Redirect(Request.Headers["Referer"]);
            }

            bool isDeleted = await _groupRepository.DeleteAsync(groupId);

            if (!isDeleted)
            {
                ErrorVM error = new()
                {
                    Title = "Delete error?!",
                    Content = "Please, try later."
                };
                return View("Error", error);
            }

            return RedirectToAction("All", "News");
        }

        [HttpGet]
        public async Task<IActionResult> Kick(string userId, string groupId)
        {
            string? currentUserId = User.GetUserId();
            if (currentUserId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }
            bool isKicked = await _groupRepository.KickUserAsync(currentUserId, userId, groupId);
            if(!isKicked)
            {
                ErrorVM error = new()
                {
                    Title = "Kick error?!",
                    Content = "Please, try later."
                };
                return View("Error", error);
            }

            return Redirect(Request.Headers["Referer"]);
        }

        [HttpGet]
        public async Task<IActionResult> Accept(string userId, string groupId)
        {
            string? currentUserId = User.GetUserId();
            if (currentUserId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }

            bool isAccepted = await _groupRepository.AcceptUserAsync(currentUserId, userId, groupId);

            if (!isAccepted)
            {
                ErrorVM error = new()
                {
                    Title = "Accept error?!",
                    Content = "Try accept later."
                };
                return View("Error", error);
            }

            return Redirect(Request.Headers["Referer"]);
        }

        [HttpGet]
        public async Task<IActionResult> Decline(string groupId, string userId = "")
        {
            string? currentUserId = User.GetUserId();
            if (currentUserId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }

            bool isDeclined;
            
            if (!string.IsNullOrEmpty(userId))
            {
                isDeclined = await _groupRepository.DeclineRequestAsync(currentUserId, userId, groupId);
            }
            else
            {
                isDeclined = await _groupRepository.DeclineRequestAsync(currentUserId, groupId);
            }
            
            if (!isDeclined)
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
        public async Task<IActionResult> Promote(string userId, string groupId, string status)
        {
            string? currentUserId = User.GetUserId();
            if (currentUserId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }
            if(!Enum.TryParse(status, true, out GroupUserStatus userStatus))
            {
                ErrorVM error = new()
                {
                    Title = "Status error?!",
                    Content = "Incorrect user status."
                };
                return View("Error", error);
            }
            bool isPromoted = await _groupRepository.PromoteUserAsync(currentUserId, userId, groupId, userStatus);

            return isPromoted ? Ok() : BadRequest();
        }
    }
}
