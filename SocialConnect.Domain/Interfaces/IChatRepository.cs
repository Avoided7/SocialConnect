using SocialConnect.Domain.Entities;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces;

public interface IChatRepository : IRepository<Chat>
{
    Task<bool> AddUserToChatAsync(string chatId, string userId);
    Task<bool> CreateMessageAsync(ChatMessage message);
}