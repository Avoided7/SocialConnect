﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Infrastructure.Data;
using System.Linq.Expressions;

namespace SocialConnect.Infrastructure.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly SocialDbContext _dbContext;
        private readonly ILogger<FriendRepository> _logger;

        public FriendRepository(SocialDbContext dbContext,
                                ILogger<FriendRepository> logger)
        {
            this._dbContext = dbContext;
            this._logger = logger;
        }
        #region GET

        public IEnumerable<FriendsCouple> Get()
        {
            return _dbContext.Friends.AsNoTracking();
        }

        public IEnumerable<FriendsCouple> Get(Expression<Func<FriendsCouple, bool>> expression)
        {
            return _dbContext.Friends.AsNoTracking().Where(expression);
        }

        public async Task<IReadOnlyCollection<FriendsCouple>> GetAsync()
        {
            return await _dbContext.Friends.AsNoTracking().ToListAsync();
        }

        public async Task<IReadOnlyCollection<FriendsCouple>> GetAsync(Expression<Func<FriendsCouple, bool>> expression)
        {
            return await _dbContext.Friends.AsNoTracking().Where(expression).ToListAsync();
        }

        public async Task<IReadOnlyCollection<User>> GetUserFriendsAsync(string userId)
        {
            try
            {
                bool isUserExist = await _dbContext.SocialUsers.AnyAsync(user => user.Id == userId);
                if (!isUserExist)
                {
                    return Enumerable.Empty<User>().ToList();
                };
                IReadOnlyCollection<User> friends = await _dbContext.Friends
                                                                            .Where(friend => (!friend.IsAgreed && (friend.FriendId == userId || friend.UserId == userId)) || 
                                                                                                        (friend.IsAgreed && friend.FriendId == userId))
                                                                            .Select(friend => _dbContext.SocialUsers.Include(user => user.Status).First(user => user.Id == (friend.FriendId == userId ? friend.UserId : friend.FriendId)))
                                                                            .ToListAsync();
                return friends;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return Enumerable.Empty<User>().ToList();
            }
        }
        public async Task<IReadOnlyCollection<User>> GetUserFriendsAsync(string userId,
                                                                 Expression<Func<FriendsCouple, bool>> expression)
        {
            try
            {
                bool isUserExist = await _dbContext.Users.AnyAsync(user => user.Id == userId);
                if (!isUserExist)
                {
                    return Enumerable.Empty<User>().ToList();
                }
                IReadOnlyCollection<User> friends = await _dbContext.Friends.Where(expression)
                                                                            .Select(friend => _dbContext.SocialUsers.Include(user => user.Friends)
                                                                                                                    .First(user => user.Id == friend.FriendId || user.Id == friend.UserId))
                                                                            .ToListAsync();
                return friends;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return Enumerable.Empty<User>().ToList();
            }
        }

        public async Task<FriendsCouple?> FirstOrDefaultAsync(Expression<Func<FriendsCouple, bool>> expression)
        {
            return await _dbContext.Friends.FirstOrDefaultAsync(expression);
        }

        #endregion

        #region CREATE

        public async Task<FriendsCouple?> CreateAsync(FriendsCouple entity)
        {
            if(entity.UserId == entity.FriendId)
            {
                return null;
            }

            FriendsCouple? friendsCouple = await _dbContext.Friends.FirstOrDefaultAsync(friend => (friend.UserId == entity.UserId &&
                                                                                                  friend.FriendId == entity.FriendId));    
            if(friendsCouple != null)
            {
                return null;
            }

            await _dbContext.Friends.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        #endregion

        #region UPDATE

        public async Task<FriendsCouple?> UpdateAsync(string id, FriendsCouple entity)
        {
            FriendsCouple? friendsCouple = await _dbContext.Friends.FirstOrDefaultAsync(friendCouple => friendCouple.Id == id);

            if (friendsCouple == null) { return null; }

            friendsCouple.FriendId = entity.FriendId;
            friendsCouple.IsAgreed = entity.IsAgreed;

            _dbContext.Friends.Update(friendsCouple);

            await _dbContext.SaveChangesAsync();

            return friendsCouple;
        }

        #endregion

        #region DELETE

        public async Task<bool> DeleteAsync(string id)
        {
            FriendsCouple? friendCouple = await _dbContext.Friends.FirstOrDefaultAsync(friendCouple => friendCouple.Id == id);

            if (friendCouple == null)
            {
                return false;
            }

            _dbContext.Friends.Remove(friendCouple);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteAsync(string userId, string friendId)
        {
            FriendsCouple? friendCouple = await _dbContext.Friends.FirstOrDefaultAsync(friendCouple => friendCouple.UserId == userId &&
                                                                                                       friendCouple.FriendId == friendId &&
                                                                                                       friendCouple.IsAgreed);
            if(friendCouple == null)
            {
                return false;
            }

            await DeleteAsync(friendCouple.Id);

            FriendsCouple? exFriend = await _dbContext.Friends.FirstOrDefaultAsync(friend => friend.UserId == friendId &&
                                                                                                       friend.FriendId == userId);

            if (exFriend == null)
            {
                return false;
            }

            exFriend.IsAgreed = false;
            _dbContext.Friends.Update(exFriend);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        #endregion

    }
}
