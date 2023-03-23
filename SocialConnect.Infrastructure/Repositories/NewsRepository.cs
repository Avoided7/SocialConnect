using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Infrastructure.Data;

namespace SocialConnect.Infrastructure.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly SocialDbContext _dbContext;
    private readonly ILogger<NewsRepository> _logger;

    public NewsRepository(SocialDbContext dbContext,
                          ILogger<NewsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<IEnumerable<News>> GetAsync()
    {
        return await _dbContext.News
                                    .Include(news => news.User)
                                    .Include(news => news.Likes)
                                    .Include(news => news.Comments)
                                    .ThenInclude(comment => comment.Likes)
                                    .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetAsync(Expression<Func<News, bool>> expression)
    {
        return await _dbContext.News
                                    .Where(expression)
                                    .Include(news => news.User)
                                    .Include(news => news.Likes)
                                    .Include(news => news.Comments)
                                    .ThenInclude(comment => comment.Likes)
                                    .ToListAsync();
    }

    public async Task<News?> FirstOrDefaultAsync(Expression<Func<News, bool>> expression)
    {
        return await _dbContext.News
                                    .Include(news => news.User)
                                    .Include(news => news.Likes)
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

    public async Task<bool> LikeAsync(string userId, string newsId)
    {
        try
        {
            if (!await _dbContext.News.AnyAsync(news => news.Id == newsId))
            {
                return false;
            }
            NewsLike? newsLike = await _dbContext.NewsLikes.FirstOrDefaultAsync(like => like.NewsId == newsId && 
                                                                                        like.UserId == userId);
            if (newsLike == null)
            {
                newsLike = new NewsLike
                {
                    UserId = userId,
                    NewsId = newsId
                };
                await _dbContext.NewsLikes.AddAsync(newsLike);
            }
            else
            {
                _dbContext.Remove(newsLike);
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> CommentAsync(string userId, string newsId, string content)
    {
        News? news = await _dbContext.News.FirstOrDefaultAsync(news => news.Id == newsId);
        if (news == null)
        {
            return false;
        }

        Comment comment = new Comment
        {
            Content = content,
            NewsId = newsId,
            UserId = userId
        };
        await _dbContext.Comments.AddAsync(comment);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LikeCommentAsync(string userId, string commentId)
    {
        try
        {
            CommentLike? commentLike =
                await _dbContext.CommentLikes.FirstOrDefaultAsync(like =>
                    like.CommentId == commentId && like.UserId == userId);
            if (commentLike == null)
            {
                commentLike = new CommentLike
                {
                    UserId = userId,
                    CommentId = commentId
                };
                await _dbContext.CommentLikes.AddAsync(commentLike);
            }
            else
            {
                _dbContext.CommentLikes.Remove(commentLike);
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }
}