using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.Domain.Extenstions;

public static class ChatExtenstion
{
    public static async Task<bool> SendMessageByGroupNameAsync(this IChatRepository chatRepository,
                                                          string groupName,
                                                          string userId,
                                                          string content)
    {
        Chat? chat = await chatRepository.FirstOrDefaultAsync(chat => chat.GroupName == groupName);

        if (chat == null)
        {
            return false;
        }
        
        ChatMessage message = new()
        {
            ChatId = chat.Id,
            UserId = userId,
            Content = content

        };

        return await chatRepository.CreateMessageAsync(message);
    }
}