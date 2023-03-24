using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;
using System.Security.Claims;

namespace SocialConnect.WebUI.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IFriendRepository _friendRepository;

        public FriendsController(IFriendRepository friendRepository)
        {
            this._friendRepository = friendRepository;
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
            return View(friends.GetFriendsStatus(friendsCouples));
        }
        
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Add(string friendId)
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest();
            }

            FriendsCouple friendRequest = new FriendsCouple
            {
                FriendId = friendId,
                UserId = userId
            };

            await _friendRepository.CreateAsync(friendRequest);

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Accept(string friendId)
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if(userId == null)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }
            bool accepted = await _friendRepository.AcceptAsync(friendId, userId);

            if(!accepted)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Decline(string friendId)
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }
            bool accepted = await _friendRepository.DeclineAsync(friendId, userId);

            if (!accepted)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }

            return Redirect(Request.Headers["Referer"]);
        }
        [HttpGet("{action}/{friendId}")]
        public async Task<IActionResult> Delete(string friendId)
        {
            string? userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }
            bool deleted = await _friendRepository.DeleteAsync(userId, friendId);

            if (!deleted)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }

            return Redirect(Request.Headers["Referer"]);
        }
    }
}
