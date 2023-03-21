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
        private readonly IMapper _mapper;

        public GroupsController(IGroupRepository groupRepository,
                                IMapper mapper)
        {
            this._groupRepository = groupRepository;
            this._mapper = mapper;
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
            IEnumerable<Group> groups = await _groupRepository.GetAsync(group => group.Users.Any(u => u.UserId == userId));
            return View(groups);
        }
        [HttpGet]
        public async Task<IActionResult> Join(string groupId)
        {
            string? userId = User.GetUserId();

            if (userId == null)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }

            bool isJoined = await _groupRepository.JoinUserAsync(groupId: groupId, userId: userId);

            if(!isJoined)
            {
                // TODO: Replace 'Bad Request' with error message/page.
                return BadRequest();
            }

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet]
        public async Task<IActionResult> Left(string groupId)
        {
            string? userId = User.GetUserId();
            if(userId == null)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }

            bool isLefted = await _groupRepository.LeftUserAsync(groupId: groupId, userId: userId);

            if (!isLefted)
            {
                // TODO: Replace 'Bad Request' with error message/page.
                return BadRequest();
            }

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet]
        public async Task<IActionResult> Info(string groupName)
        {
            Group? group = await _groupRepository.FirstOrDefaultAsync(group => group.Name == groupName);

            if (group == null)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }

            return View(group);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(GroupVM groupVM)
        {
            string? userId = User.GetUserId();
            if (userId == null)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(groupVM);
            }

            Group group = _mapper.Map<Group>(groupVM);
            Group? createdGroup = await _groupRepository.CreateAsync(group);
            if (createdGroup == null)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }

            bool isJoined = await _groupRepository.JoinUserAsync(createdGroup.Id, userId);

            return RedirectToAction(nameof(Info), new { groupName = group.Name });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string groupId)
        {
            string? userId = User.GetUserId();
            if (userId == null)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }
            bool isFounder = await _groupRepository.IsFounderAsync(groupId, userId);

            if (!isFounder)
            {
                return Redirect(Request.Headers["Referer"]);
            }

            bool isDeleted = await _groupRepository.DeleteAsync(groupId);

            if (!isDeleted)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Kick(string userId, string groupId)
        {
            string? currentUserId = User.GetUserId();
            if (currentUserId == null)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }
            bool isKicked = await _groupRepository.KickUserAsync(currentUserId, userId, groupId);
            if(!isKicked)
            {
                // TODO: Replace 'Bad Request' with error page/message.
                return BadRequest();
            }

            return Redirect(Request.Headers["Referer"]);
        }

        [HttpGet]
        public async Task<IActionResult> Promote(string userId, string groupId, string status)
        {
            string? currentUserId = User.GetUserId();
            if (currentUserId == null)
            {
                // TODO: Replace 'Bad Request' with error message.
                return BadRequest();
            }
            if(!Enum.TryParse(status, true, out GroupUserStatus userStatus))
            {
                // TODO: Replace 'Bad Request' with error message.
                return BadRequest();
            }
            bool isPromoted = await _groupRepository.PromoteUserAsync(currentUserId, userId, groupId, userStatus);

            return isPromoted ? Ok() : BadRequest();
        }
    }
}
