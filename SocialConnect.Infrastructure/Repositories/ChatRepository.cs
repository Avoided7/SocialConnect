using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Infrastructure.Data;

namespace SocialConnect.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly SocialDbContext _dbContext;

    public ChatRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #region GET
    
    public IEnumerable<Chat> Get()
    {
        return _dbContext.Chats
            .AsNoTracking()
            .Include(chat => chat.Users)
                .ThenInclude(user => user.User)
            .Include(chat => chat.Messages);
    }

    public IEnumerable<Chat> Get(Expression<Func<Chat, bool>> expression)
    {
        return _dbContext.Chats
            .AsNoTracking()
            .Include(chat => chat.Users)
                .ThenInclude(user => user.User)
            .Include(chat => chat.Messages)
            .Where(expression);
    }
    public async Task<IReadOnlyCollection<Chat>> GetAsync()
    {
        return await _dbContext.Chats
            .AsNoTracking()
            .Include(chat => chat.Users)
                .ThenInclude(user => user.User)
            .Include(chat => chat.Messages)
            .ToListAsync();
    }
    public async Task<IReadOnlyCollection<Chat>> GetAsync(Expression<Func<Chat, bool>> expression)
    {
        return await _dbContext.Chats
            .AsNoTracking()
            .Include(chat => chat.Users)
                .ThenInclude(user => user.User)
            .Include(chat => chat.Messages)
            .Where(expression)
            .ToListAsync();
    }
    public async Task<Chat?> FirstOrDefaultAsync(Expression<Func<Chat, bool>> expression)
    {
        return await _dbContext.Chats
            .AsNoTracking()
            .Include(chat => chat.Users)
                .ThenInclude(user => user.User)
            .Include(chat => chat.Messages)
            .FirstOrDefaultAsync(expression);
    }

    #endregion

    #region CREATE
    
    public async Task<Chat?> CreateAsync(Chat entity)
    {
        await _dbContext.Chats.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    #endregion

    #region UPDATE

    public async Task<Chat?> UpdateAsync(string id, Chat entity)
    {
        Chat? chat = await _dbContext.Chats.FirstOrDefaultAsync(chat => chat.Id == id);
        if (chat == null)
        {
            return null;
        }
        
        /* Update something */

        await _dbContext.SaveChangesAsync();

        return chat;
    }

    #endregion

    #region DELETE

    public async Task<bool> DeleteAsync(string id)
    {
        Chat? chat = await _dbContext.Chats.FirstOrDefaultAsync(chat => chat.Id == id);
        if (chat == null)
        {
            return false;
        }

        _dbContext.Chats.Remove(chat);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    #endregion
    
    public async Task<bool> AddUserToChatAsync(string chatId, string userId)
    {
        if (!await _dbContext.Chats.AnyAsync(chat => chat.Id == chatId))
        {
            return false;
        }

        bool existUser = await _dbContext.ChatsUsers.AnyAsync(chatUser => chatUser.ChatId == chatId &&
                                                                          chatUser.UserId == userId);
        if (existUser)
        {
            return false;
        }

        ChatUser chatUser = new()
        {
            ChatId = chatId,
            UserId = userId
        };

        await _dbContext.ChatsUsers.AddAsync(chatUser);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CreateMessageAsync(ChatMessage message)
    {
        await _dbContext.ChatsMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}