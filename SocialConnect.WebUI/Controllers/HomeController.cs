using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Entitities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;
using SocialConnect.WebUI.ViewModels;
using SocialConnect.WebUI.ViewModels.Enums;
using System.Collections.Generic;
using System.Security.Claims;

namespace SocialConnect.WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IMapper _mapper;

        public HomeController(ILogger<HomeController> logger,
                              IUserRepository userRepository,
                              IFriendRepository friendRepository,
                              IMapper mapper)
        {
            this._logger = logger;
            this._userRepository = userRepository;
            this._friendRepository = friendRepository;
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

    }
}