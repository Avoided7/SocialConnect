using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Infrastructure.Data;
using System.Linq.Expressions;

namespace SocialConnect.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly SocialDbContext _dbContext;
        private readonly ILogger<GroupRepository> _logger;
        private readonly INewsRepository _newsRepository;

        public GroupRepository(SocialDbContext dbContext,
                               ILogger<GroupRepository> logger,
                               INewsRepository newsRepository)
        {
            this._dbContext = dbContext;
            this._logger = logger;
            this._newsRepository = newsRepository;
        }

        #region GET

        public async Task<Group?> FirstOrDefaultAsync(Expression<Func<Group, bool>> expression)
        {
            try
            {
                return await _dbContext.Groups
                    .Include(group => group.Users)
                        .ThenInclude(groupUser => groupUser.User)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Comments)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Likes)
                    .FirstOrDefaultAsync(expression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public Task<IQueryable<Group>> GetAsync()
        {
            try
            {
                return Task.Run(() => _dbContext.Groups
                    .Include(group => group.Users)
                        .ThenInclude(groupUser => groupUser.User)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Comments)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Likes)
                    .AsNoTracking());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Task.Run(() => Enumerable.Empty<Group>().AsQueryable());
            }
        }

        public Task<IQueryable<Group>> GetAsync(Expression<Func<Group, bool>> expression)
        {
            try
            {
                return Task.Run(() => _dbContext.Groups
                    .Include(group => group.Users)
                        .ThenInclude(groupUser => groupUser.User)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Comments)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Likes)
                    .AsNoTracking()
                    .Where(expression));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Task.Run(() => Enumerable.Empty<Group>().AsQueryable());
            }
        }

        #endregion

        #region CREATE

        public async Task<Group?> CreateAsync(Group entity)
        {
            try
            {
                bool isExisted = await _dbContext.Groups.AnyAsync(group => group.Name.ToLower() == entity.Name.ToLower());

                if(isExisted)
                {
                    return null;
                }

                await _dbContext.Groups.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        #endregion

        #region DELETE

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                Group? group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id);
                if(group == null)
                {
                    return false;
                }

                IEnumerable<News> groupNews = _dbContext.News.Where(news => news.GroupId == group.Id);
                await _newsRepository.RemoveRangeAsync(groupNews);
                
                IEnumerable<GroupUser> groupUsers = _dbContext.GroupUsers.Where(groupUsers => groupUsers.GroupId == group.Id);
                _dbContext.GroupUsers.RemoveRange(groupUsers);
                
                _dbContext.Groups.Remove(group);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }


        #endregion

        #region UPDATE

        public async Task<Group?> UpdateAsync(string id, Group entity)
        {
            try
            {
                Group? group = await _dbContext.Groups.FirstOrDefaultAsync(group => group.Id == id);
                if (group == null)
                {
                    return null;
                }
                group.Name = entity.Name;
                group.Description = entity.Description;

                _dbContext.Groups.Update(group);
                await _dbContext.SaveChangesAsync();

                return group;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }


        #endregion

        #region CUSTOM METHODS
        
        public async Task<bool> JoinUserAsync(string groupId, string userId)
        {
            try
            {
                Group? group = await FirstOrDefaultAsync(group => group.Id == groupId);
                if (group == null)
                {
                    return false;
                }
                GroupUser? existedGroupUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser => groupUser.GroupId == groupId &&
                                                                                                           groupUser.UserId == userId);
                if (existedGroupUser != null)
                {
                    return false;
                }

                existedGroupUser = new GroupUser
                {
                    GroupId = groupId,
                    UserId = userId,
                    UserStatus = GroupUserStatus.User
                };

                if(group.UserCount == 0)
                {
                    existedGroupUser.UserStatus = GroupUserStatus.Founder;
                    existedGroupUser.IsAgreed = true;
                }

                _dbContext.GroupUsers.Add(existedGroupUser);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> LeftUserAsync(string groupId, string userId)
        {
            try
            {
                Group? group = await FirstOrDefaultAsync(group => group.Id == groupId);
                if (group == null)
                {
                    return false;
                }
                if(group.UserCount == 1)
                {
                    return await DeleteAsync(group.Id);
                }
                GroupUser? existedGroupUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser => groupUser.GroupId == groupId &&
                                                                                                           groupUser.UserId == userId);
                if (existedGroupUser == null || existedGroupUser.UserStatus == GroupUserStatus.Founder)
                {
                    return false;
                }

                _dbContext.GroupUsers.Remove(existedGroupUser);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> PromoteUserAsync(string currentUserId, string userId, string groupId, GroupUserStatus newStatus)
        {
            try
            {
                if(newStatus == GroupUserStatus.Founder)
                {
                    return false;
                }
                Group? group = await FirstOrDefaultAsync(group => group.Id == groupId);
                if (group == null)
                {
                    return false;
                }

                GroupUser? promotedUser = group.Users.FirstOrDefault(groupUser => groupUser.UserId == userId);
                if (promotedUser == null || promotedUser.UserStatus == GroupUserStatus.Founder)
                {
                    return false;
                }

                GroupUser? founder = group.Users.FirstOrDefault(groupUser => groupUser.UserId == currentUserId);

                if (founder == null || founder.UserStatus != GroupUserStatus.Founder)
                {
                    return false;
                }

                promotedUser.UserStatus = newStatus;
                _dbContext.GroupUsers.Update(promotedUser);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> AcceptUserAsync(string currenUserId, string userId, string groupId)
        {
            try
            {
                Group? group = await _dbContext.Groups.Include(group => group.Users)
                    .FirstOrDefaultAsync(group => group.Id == groupId);

                if (group == null)
                {
                    return false;
                }

                GroupUser? currentUser = group.Users.FirstOrDefault(user => user.UserId == currenUserId);

                if (currentUser == null || currentUser.UserStatus == GroupUserStatus.User)
                {
                    return false;
                }

                GroupUser? acceptedUser = group.Users.FirstOrDefault(user => user.UserId == userId);

                if (acceptedUser == null || acceptedUser.IsAgreed)
                {
                    return false;
                }

                acceptedUser.IsAgreed = true;
                _dbContext.GroupUsers.Update(acceptedUser);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeclineRequestAsync(string userId, string groupId)
        {
            try
            {
                GroupUser? groupUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser =>
                    groupUser.UserId == userId && groupUser.GroupId == groupId);
                if (groupUser == null)
                {
                    return false;
                }

                if (groupUser.IsAgreed)
                {
                    return false;
                }

                _dbContext.GroupUsers.Remove(groupUser);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeclineRequestAsync(string currentUserId, string userId, string groupId)
        {
            try
            {
                GroupUser? currentUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser =>
                    groupUser.UserId == currentUserId && groupUser.GroupId == groupId);
                if (currentUser == null || currentUser.UserStatus == GroupUserStatus.User)
                {
                    return false;
                }

                GroupUser? declinedUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser =>
                    groupUser.UserId == userId && groupUser.GroupId == groupId);
                
                if (declinedUser == null || declinedUser.IsAgreed)
                {
                    return false;
                }

                _dbContext.GroupUsers.Remove(declinedUser);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> KickUserAsync(string currentUserId, string userId, string groupId)
        {
            try
            {
                Group? group = await _dbContext.Groups.Include(group => group.Users)
                                                      .FirstOrDefaultAsync(group => group.Id == groupId);

                if (group == null)
                {
                    return false;
                }

                GroupUser? currentUser = group.Users.FirstOrDefault(groupUser => groupUser.UserId == currentUserId);

                if (currentUser == null || currentUser.UserStatus == GroupUserStatus.User)
                {
                    return false;
                }

                GroupUser? kickedUser = group.Users.FirstOrDefault(groupUser => groupUser.UserId == userId);

                if (kickedUser == null || kickedUser.UserStatus == GroupUserStatus.Founder)
                {
                    return false;
                }
                if (kickedUser.UserStatus == GroupUserStatus.Admin && currentUser.UserStatus != GroupUserStatus.Founder)
                {
                    return false;
                }

                _dbContext.GroupUsers.Remove(kickedUser);
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
}
