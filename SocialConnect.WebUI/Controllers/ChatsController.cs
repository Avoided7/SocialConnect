using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialConnect.Domain.Entities;
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

        public ChatsController(IChatRepository chatRepository,
                               IUserRepository userRepository,
                               IHubContext<ChatHub> chatContext)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _chatContext = chatContext;
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
        
        [HttpGet]
        public async Task<IActionResult> Index(string chatId)
        {
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
    }
}
