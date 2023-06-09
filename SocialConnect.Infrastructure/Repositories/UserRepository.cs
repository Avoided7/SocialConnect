﻿using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialConnect.Infrastructure.Data;

namespace SocialConnect.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly SocialDbContext _dbContext;

        public UserRepository(ILogger<UserRepository> logger,
                              SocialDbContext dbContext)
        {
            this._logger = logger;
            _dbContext = dbContext;
        }

        #region GET

        public IEnumerable<User> Get()
        {
            return _dbContext.SocialUsers
                .AsNoTracking()
                .Include(user => user.Friends)
                .Include(user => user.Status)
                .Include(user => user.News)
                    .ThenInclude(news => news.Contents)
                .Include(user => user.News)
                    .ThenInclude(news => news.Likes)
                .Include(user => user.News)
                    .ThenInclude(news => news.Comments)
                        .ThenInclude(comment => comment.Likes)
                .Include(user => user.Groups)
                    .ThenInclude(groupUser => groupUser.Group);
        }

        public IEnumerable<User> Get(Expression<Func<User, bool>> expression)
        {
            return _dbContext.SocialUsers
                .AsNoTracking()
                .Include(user => user.Friends)
                .Include(user => user.Status)
                .Include(user => user.News)
                    .ThenInclude(news => news.Contents)
                .Include(user => user.News)
                    .ThenInclude(news => news.Likes)
                .Include(user => user.News)
                    .ThenInclude(news => news.Comments)
                        .ThenInclude(comment => comment.Likes)
                .Include(user => user.Groups)
                    .ThenInclude(groupUser => groupUser.Group)
                .Where(expression);
        }

        public async Task<IReadOnlyCollection<User>> GetAsync()
        {
            return await _dbContext.SocialUsers
                                           .AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.Status)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Contents)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .ToListAsync();
        }

        public async Task<IReadOnlyCollection<User>> GetAsync(Expression<Func<User, bool>> expression)
        {
            return await _dbContext.SocialUsers
                                           .AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.Status)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Contents)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .Where(expression)
                                           .ToListAsync();
        }

        public async Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> expression)
        {
            return await _dbContext.SocialUsers
                                           .AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.Status)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Contents)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .FirstOrDefaultAsync(expression);
        }

        #endregion

        #region CREATE

        public async Task<User?> CreateAsync(User entity)
        {
            await _dbContext.SocialUsers.AddAsync(entity);

            Status status = new()
            {
                UserId = entity.Id,
            };
            
            await _dbContext.UsersStatus.AddAsync(status);
            
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        #endregion

        #region UPDATE

        public async Task<User?> UpdateAsync(string id, User entity)
        {
            User? user = await _dbContext.SocialUsers.FirstOrDefaultAsync(user => user.Id == id);

            if(user == null)
            {
                return null;
            }

            user.Firstname = entity.Firstname;
            user.Lastname = entity.Lastname;
            user.UserName = entity.UserName;
            user.Email = entity.Email;
            user.DateOfBirth = entity.DateOfBirth;
            user.Gender = entity.Gender;

            _dbContext.SocialUsers.Update(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }


        #endregion

        #region DELETE

        public async Task<bool> DeleteAsync(string id)
        {
            User? user = await _dbContext.SocialUsers.FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return false;
            }
            
            _dbContext.SocialUsers.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(User entity)
        {
            return await DeleteAsync(entity.Id);
        }

        #endregion

        #region CUSTOM METHODS
        
        public async Task<User?> FindByIdAsync(string id)
        {
            User? user = await FirstOrDefaultAsync(user => user.Id == id);

            return user;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            User? user = await FirstOrDefaultAsync(user => user.UserName == username);

            return user;
        }

        
        public async Task<Status?> GetUserStatusAsync(string userId)
        {
            Status? status = await _dbContext.UsersStatus.FirstOrDefaultAsync(userStatus => userStatus.UserId == userId);

            return status;
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, Status status)
        {
            Status? existedStatus = await _dbContext.UsersStatus.FirstOrDefaultAsync(userStatus => userStatus.UserId == userId);

            if (existedStatus == null)
            {
                return false;
            }

            existedStatus.IsOnline = status.IsOnline;
            existedStatus.LastSeenOnline = status.LastSeenOnline;

            _dbContext.Update(existedStatus);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        
        #endregion
    }
}
