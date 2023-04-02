using SocialConnect.Domain.Entities;
using SocialConnect.Shared;

namespace SocialConnect.Domain.Interfaces;

public interface IChatRepository : IRepository<Chat>
{
    Task<bool> AddUserToChatAsync(string chatId, string userId);
    Task<bool> CreateMessageAsync(ChatMessage message);
    Task<bool> DeleteMessageAsync(string messageId);
    Task<ChatMessage?> GetMessageAsync(string messageId);
    Task<bool> AddViewToMessageAsync(MessageView view);
    Task<bool> AddRangeViewsToMessageAsync(IEnumerable<MessageView> views);
}