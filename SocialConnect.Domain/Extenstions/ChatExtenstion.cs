using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.Domain.Extenstions;

public static class ChatExtenstion
{
    public static async Task<string> SendMessageByGroupNameAsync(this IChatRepository chatRepository,
                                                          string groupName,
                                                          string userId,
                                                          string content)
    {
        Chat? chat = await chatRepository.FirstOrDefaultAsync(chat => chat.GroupName == groupName);

        if (chat == null)
        {
            return string.Empty;
        }
        
        ChatMessage message = new()
        {
            ChatId = chat.Id,
            UserId = userId,
            Content = content

        };

        await chatRepository.CreateMessageAsync(message);
        
        return message.Id;
    }

    public static async Task<string?> GetGroupNameByChatIdAsync(this IChatRepository chatRepository,
        string chatId)
    {
        Chat? chat = await chatRepository.FirstOrDefaultAsync(chat => chat.Id == chatId);

        return chat?.GroupName;
    }

    public static async Task<bool> ReadAllMessagesAsync(this IChatRepository chatRepository,
        string chatId,
        string userId)
    {
        Chat? chat = await chatRepository.FirstOrDefaultAsync(chat => chat.Id == chatId);
        if (chat == null)
        {
            return false;
        }

        IEnumerable<MessageView> messages = chat.Messages.Where(message => message.Views.All(view => view.UserId != userId))
                                                         .Select(message => new MessageView() { MessageId = message.Id, UserId = userId});

        return await chatRepository.AddRangeViewsToMessageAsync(messages);
    }
}