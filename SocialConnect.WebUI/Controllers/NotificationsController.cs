using Microsoft.AspNetCore.Mvc;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;

namespace SocialConnect.WebUI.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IChatRepository _chatRepository;

        public NotificationsController(IFriendRepository friendRepository,
                                      IChatRepository chatRepository)
        {
            this._friendRepository = friendRepository;
            this._chatRepository = chatRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChatsCount()
        {
            string? userId = User.GetUserId();
            if (userId == null)
            {
                return BadRequest();
            }

            IReadOnlyCollection<Chat> chats = await _chatRepository.GetAsync(chat =>
                            chat.Users.Any(chatUser => chatUser.UserId == userId) &&
                            chat.Messages.Any(message => message.Views.All(view => view.UserId != userId)));

            return Ok(chats.Count());
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFriendsRequestsCount()
        {
            string? userId = User.GetUserId();
            if(userId == null)
            {
                return BadRequest();
            }

            IReadOnlyCollection<FriendsCouple> friendsRequests = await _friendRepository.GetAsync(userFriend => !userFriend.IsAgreed && 
                                                                                                                 userFriend.FriendId == userId);
            return Ok(friendsRequests.Count());
        }
    }
}
