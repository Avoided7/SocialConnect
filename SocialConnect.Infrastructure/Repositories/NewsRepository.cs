using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
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

    #region GET

    public Task<IQueryable<News>> GetAsync()
    {
        return Task.Run(() => _dbContext.News
            .Include(news => news.User)
            .Include(news => news.Group)
                .ThenInclude(group => group == null ? default : group.Users)
            .Include(news => news.Likes)
            .Include(news => news.Contents)
            .Include(news => news.Comments)
                .ThenInclude(comment => comment.Likes)
            .Include(news => news.Comments)
                .ThenInclude(comment => comment.User)
            .AsNoTracking());
    }

    public Task<IQueryable<News>> GetAsync(Expression<Func<News, bool>> expression)
    {
        return Task.Run(() => _dbContext.News
            .Include(news => news.User)
            .Include(news => news.Group)
                .ThenInclude(group => group == null ? default : group.Users)
            .Include(news => news.Likes)
            .Include(news => news.Contents)
            .Include(news => news.Comments)
                .ThenInclude(comment => comment.Likes)
            .Include(news => news.Comments)
                .ThenInclude(comment => comment.User)
            .Where(expression)
            .AsNoTracking());
    }

    public async Task<News?> FirstOrDefaultAsync(Expression<Func<News, bool>> expression)
    {
        return await _dbContext.News
            .Include(news => news.User)
            .Include(news => news.Group)
                .ThenInclude(group => group == null ? default : group.Users)
            .Include(news => news.Likes)
            .Include(news => news.Contents)
            .Include(news => news.Comments)
                .ThenInclude(comment => comment.Likes)
            .Include(news => news.Comments)
                .ThenInclude(comment => comment.User)
            .FirstOrDefaultAsync(expression);
    }

    #endregion

    #region CREATE

    public async Task<News?> CreateAsync(News entity)
    {
        _dbContext.AddRange(entity.Contents);
        EntityEntry<News> entityEntry = await _dbContext.News.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    #endregion

    #region UPDATE

    public async Task<News?> UpdateAsync(string id, News entity)
    {
        News? news = await _dbContext.News.Include(news => news.Contents)
                                          .FirstOrDefaultAsync(news => news.Id == id);
        
        if (news == null)
        {
            return null;
        }

        // I don't wanna update all content, only text.

        NewsContent? oldTextContent = news.Contents.FirstOrDefault(content => content.Type == NewsContentType.Text);
        if (oldTextContent != null)
        {
            _dbContext.NewsContents.Remove(oldTextContent);
        }

        NewsContent? newTextContent = entity.Contents.FirstOrDefault(content => content.Type == NewsContentType.Text);
        if (newTextContent != null)
        {
            await _dbContext.NewsContents.AddAsync(newTextContent);
        }

        await _dbContext.SaveChangesAsync();
        
        return news;
    }

    #endregion

    #region DELETE

    public async Task<bool> DeleteAsync(string id)
    {
        News? news = await _dbContext.News.Include(news => news.Contents)
                                          .FirstOrDefaultAsync(news => news.Id == id);
        if (news == null)
        {
            return false;
        }

        IEnumerable<NewsContent> newsContents = news.Contents;
        
        IQueryable<Comment> comments = _dbContext.Comments.Where(comment => comment.NewsId == id);
        IQueryable<CommentLike> commentLikes = _dbContext.CommentLikes.Where(like => comments.Any(comment => comment.Id == like.CommentId));
        
        IQueryable<NewsLike> newsLikes = _dbContext.NewsLikes.Where(like => like.NewsId == id);
        
        // Deleting relations
        _dbContext.NewsLikes.RemoveRange(newsLikes);
        _dbContext.CommentLikes.RemoveRange(commentLikes);
        _dbContext.Comments.RemoveRange(comments);
        _dbContext.NewsContents.RemoveRange(newsContents);
        
        // Deleting main entity
        _dbContext.News.Remove(news);
        
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<News> news)
    {
        await Task.WhenAll(news.Select(currentNews => DeleteAsync(currentNews.Id)));

        return true;
    }
    
    #endregion

    #region CUSTOM METHODS
    
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
    
    #endregion
}