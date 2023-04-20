using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;
using System.Security.Claims;
using SocialConnect.WebUI.ViewModels;
using SocialConnect.Domain.Extenstions;
using SocialConnect.WebUI.Enums;
using Microsoft.AspNetCore.SignalR;
using SocialConnect.WebUI.Hubs;

namespace SocialConnect.WebUI.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IHubContext<NotificationHub> _notificationContext;

        public FriendsController(IFriendRepository friendRepository,
                                 IHubContext<NotificationHub> notificationContext)
        {
            this._friendRepository = friendRepository;
            this._notificationContext = notificationContext;
        }
        [HttpGet]
        public async Task<IActionResult> All(string type = "all")
        {
            string? userId = User.GetUserId();

            if (userId == null )
            {
                return BadRequest();
            }
            IEnumerable<User> friends = await _friendRepository.GetUserFriendsAsync(userId);
            IEnumerable<FriendsCouple> friendsCouples = await _friendRepository.GetAsync(friend => friend.FriendId == userId || friend.UserId == userId);

            IEnumerable<UserVM> friendsWithStatus = friends.GetFriendsStatus(friendsCouples);

            FriendVM friendsVM = new()
            {
                Friends = friendsWithStatus.Where(user => user.Status == FriendStatus.Friend).ToList(),
                SendedRequest = friendsWithStatus.Where(user => user.Status == FriendStatus.SendedRequest).ToList(),
                WaitedResponse = friendsWithStatus.Where(user => user.Status == FriendStatus.WaitedResponse).ToList(),
            };

            return View(friendsVM);
        }
        
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Add(string friendId)
        {
            string? userId = User.GetUserId();
            if (userId == null)
            {
                return BadRequest();
            }

            FriendsCouple friendRequest = new FriendsCouple
            {
                FriendId = friendId,
                UserId = userId
            };

            FriendsCouple? friendsCouple = await _friendRepository.CreateAsync(friendRequest);

            if(friendsCouple != null)
            {
                await _notificationContext.Clients.User(friendsCouple.FriendId).SendAsync("Receive", NotificationType.FriendRequest.ToString(), userId);
            }

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Accept(string friendId)
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if(userId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }
            bool accepted = await _friendRepository.AcceptAsync(friendId, userId);

            if(!accepted)
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
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Decline(string friendId)
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }
            bool accepted = await _friendRepository.DeclineAsync(friendId, userId);

            if (!accepted)
            {
                ErrorVM error = new()
                {
                    Title = "Error?!",
                    Content = "Please, try later."
                };
                return View("Error", error);
            }

            await _notificationContext.Clients.User(friendId).SendAsync("Receive", NotificationType.FriendRequest.ToString(), friendId);

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Delete(string friendId)
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }
            bool deleted = await _friendRepository.DeleteAsync(userId, friendId);

            if (!deleted)
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
}
