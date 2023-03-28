using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Shared.Models;
using SocialConnect.Infrastructure.Interfaces;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Constants;
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

        public Task<IQueryable<User>> GetAsync()
        {
            return Task.Run(() => _dbContext.SocialUsers.AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Contents)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .AsNoTracking());
        }

        public Task<IQueryable<User>> GetAsync(Expression<Func<User, bool>> expression)
        {
            return Task.Run(() => _dbContext.SocialUsers.AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Contents)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .AsNoTracking()
                                           .Where(expression));
        }

        public async Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> expression)
        {
            return await _dbContext.SocialUsers.AsNoTracking()
                                           .Include(user => user.Friends)
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

        #endregion
    }
}
