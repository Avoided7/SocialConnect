using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Infrastructure.Data;

namespace SocialConnect.Infrastructure.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly SocialDbContext _dbContext;

    public NewsRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<News>> GetAsync()
    {
        return await _dbContext.News
                                    .Include(news => news.User)
                                    .Include(news => news.Comments)
                                    .ThenInclude(comment => comment.Likes)
                                    .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetAsync(Expression<Func<News, bool>> expression)
    {
        return await _dbContext.News
                                    .Where(expression)
                                    .Include(news => news.User)
                                    .Include(news => news.Comments)
                                    .ThenInclude(comment => comment.Likes)
                                    .ToListAsync();
    }

    public async Task<News?> FirstOrDefaultAsync(Expression<Func<News, bool>> expression)
    {
        return await _dbContext.News
                                    .Include(news => news.User)
                                    .Include(news => news.Comments)
                                    .ThenInclude(comment => comment.Likes)
                                    .FirstOrDefaultAsync(expression);
    }

    public async Task<News?> CreateAsync(News entity)
    {
        EntityEntry<News> entityEntry = await _dbContext.News.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<News?> UpdateAsync(string id, News entity)
    {
        News? news = await _dbContext.News.FirstOrDefaultAsync(news => news.Id == id);

        if (news == null)
        {
            return null;
        }

        news.Title = entity.Title;
        news.Description = entity.Description;

        _dbContext.News.Update(news);
        await _dbContext.SaveChangesAsync();
        
        return news;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        News? news = await _dbContext.News.FirstOrDefaultAsync(news => news.Id == id);
        if (news == null)
        {
            return false;
        }

        _dbContext.News.Remove(news);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}