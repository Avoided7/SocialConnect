using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;
using System.Security.Claims;

namespace SocialConnect.WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IMapper _mapper;

        public HomeController(ILogger<HomeController> logger,
                              IUserRepository userRepository,
                              IFriendRepository friendRepository,
                              IGroupRepository groupRepository,
                              IMapper mapper)
        {
            this._logger = logger;
            this._userRepository = userRepository;
            this._friendRepository = friendRepository;
            this._groupRepository = groupRepository;
            this._mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Users()
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest();
            }

            IEnumerable<User> friends = await _userRepository.GetAsync(user => user.Id != userId);
            IEnumerable<FriendsCouple> friendsCouples = await _friendRepository.GetAsync(friend => friend.FriendId == userId ||
                                                                                                   friend.UserId == userId);
            

            return View(friends.GetFriendsStatus(friendsCouples));
        }
        public async Task<IActionResult> Groups()
        {
            IEnumerable<Group> groups = await _groupRepository.GetAsync();

            return View(groups);
        }

    }
}