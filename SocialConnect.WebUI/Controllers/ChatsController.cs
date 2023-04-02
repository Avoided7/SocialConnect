using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.Extenstions;
using SocialConnect.WebUI.Hubs;
using SocialConnect.WebUI.ViewModels;

namespace SocialConnect.WebUI.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<ChatHub> _chatContext;
        private readonly IHubContext<NotificationHub> _notificationContext;

        public ChatsController(IChatRepository chatRepository,
                               IUserRepository userRepository,
                               IHubContext<ChatHub> chatContext,
                               IHubContext<NotificationHub> notificationContext)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _chatContext = chatContext;
            _notificationContext = notificationContext;
        }

        public async Task<IActionResult> All()
        {
            string? userId = User.GetUserId();
            if (userId == null)
            {
                // TODO: Replace with error page.
                return BadRequest();
            }

            IReadOnlyCollection<Chat> chats = await _chatRepository.GetAsync(chat => chat.Users.Any(user => user.UserId == userId));
            
            return View(chats);
        }
        
        [HttpGet("Chat/{chatId}")]
        public async Task<IActionResult> Index(string chatId)
        {
            string? userId = User.GetUserId();
            if (userId == null)
            {
                // TODO: Replace 'Bad Request' with error page.
                return BadRequest();
            }
            Chat? chat = await _chatRepository.FirstOrDefaultAsync(user => user.Id == chatId);
            if(chat == null)
            {
                ErrorVM error = new ErrorVM()
                {
                    Title = "User error?!",
                    Content = "User not found."
                };

                return View("Error", error);
            }
            chat.Messages = chat.Messages.OrderBy(message => message.WrittenAt).ToList();
            await _chatRepository.ReadAllMessagesAsync(chat.Id, userId);
            return View(chat);
        }

        public async Task<IActionResult> Create(string username)
        {
            User? user = await _userRepository.FindByUsernameAsync(username);
            string? currentUserId = User.GetUserId();
            if (user == null || currentUserId == null || currentUserId == user.Id)
            {
                return BadRequest();
            }

            Chat chat = new()
            {
                GroupName = Guid.NewGuid().ToString(),
                IsCoupleChat = true
            };
            await _chatRepository.CreateAsync(chat);
            
            await _chatRepository.AddUserToChatAsync(chat.Id, user.Id);
            await _chatRepository.AddUserToChatAsync(chat.Id, currentUserId);

            return RedirectToAction(nameof(Index), new { chatId = chat.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageVM chatMessage)
        {
            string message = chatMessage.Message.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest();
            }
            string username = this.HttpContext.User?.Identity?.Name ?? "Unknown";
            string? userId = this.HttpContext.User?.GetUserId();

            if (userId == null)
            {
                return BadRequest();
            }
            
            string groupName = chatMessage.GroupName;

            Chat? chat = await _chatRepository.FirstOrDefaultAsync(chat => chat.GroupName == groupName);
            if (chat == null)
            {
                return BadRequest();
            }
            
            string messageId = await _chatRepository.SendMessageByGroupNameAsync(groupName, userId, message);
            
            await _chatContext.Clients.Group(groupName).SendAsync("Receive", username, messageId, message);

            IReadOnlyCollection<string> users = chat.Users
                      .Where(chatUser => chatUser.UserId != userId)
                      .Select(user => user.UserId!)
                      .ToList();

            await _notificationContext.Clients.Users(users).SendAsync("receive", message);
            
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            string? userId = User.GetUserId();
            if (userId == null)
            {
                return BadRequest();
            }
            ChatMessage? message = await _chatRepository.GetMessageAsync(messageId);

            if (message == null || message.UserId != userId)
            {
                return BadRequest();
            }
            
            bool isDeleted = await _chatRepository.DeleteMessageAsync(messageId);
            if (!isDeleted)
            {
                return BadRequest();
            }

            string? groupName = await _chatRepository.GetGroupNameByChatIdAsync(message.ChatId);
            if (groupName == null)
            {
                throw new Exception("Undefined group name.");
            }

            await _chatContext.Clients.Group(groupName).SendAsync("Delete", messageId);
            return Ok();
        }
    }
}
