using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Infrastructure.Data;
using System.Linq.Expressions;
using Microsoft.VisualBasic;

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

        public IEnumerable<Group> Get()
        {
            return _dbContext.Groups.AsNoTracking();
        }

        public IEnumerable<Group> Get(Expression<Func<Group, bool>> expression)
        {
            return _dbContext.Groups.AsNoTracking().Where(expression);
        }

        public async Task<Group?> FirstOrDefaultAsync(Expression<Func<Group, bool>> expression)
        {
            try
            {
                return await _dbContext.Groups
                    .Include(group => group.Users)
                        .ThenInclude(groupUser => groupUser.User)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Contents)
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

        public async Task<IReadOnlyCollection<Group>> GetAsync()
        {
            try
            {
                return await _dbContext.Groups
                    .Include(group => group.Users)
                        .ThenInclude(groupUser => groupUser.User)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Contents)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Comments)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Likes)
                    .AsNoTracking()
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Enumerable.Empty<Group>().ToList();
            }
        }

        public async Task<IReadOnlyCollection<Group>> GetAsync(Expression<Func<Group, bool>> expression)
        {
            try
            {
                return await _dbContext.Groups
                    .Include(group => group.Users)
                        .ThenInclude(groupUser => groupUser.User)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Contents)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Comments)
                    .Include(group => group.News)
                        .ThenInclude(news => news.Likes)
                    .AsNoTracking()
                    .Where(expression)
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Enumerable.Empty<Group>().ToList();
            }
        }

        #endregion

        #region CREATE

        public async Task<Group?> CreateAsync(Group entity)
        {
            try
            {
                Group? existedGroup = await _dbContext.Groups.FirstOrDefaultAsync(group => group.Name.ToLower() == entity.Name.ToLower());

                if(existedGroup != null)
                {
                    return existedGroup;
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
        
        public async Task<GroupUser?> UpdateGroupUserAsync(string groupId, string userId, GroupUser user)
        {
            GroupUser? groupUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser => groupUser.GroupId == groupId &&
                                                                                                groupUser.UserId == userId);
            if (groupUser  == null)
            {
                return null;
            }

            groupUser.IsAgreed = user.IsAgreed;
            groupUser.UserStatus = user.UserStatus;

            _dbContext.GroupUsers.Update(groupUser);
            await _dbContext.SaveChangesAsync();

            return groupUser;
        }


        #endregion

        #region GROUP USER METHODS
        
        public async Task<bool> AddUserToGroupAsync(string groupId, GroupUser groupUser)
        {
            if(!await _dbContext.Groups.AnyAsync(group => group.Id == groupId))
            {
                return false;
            }

            groupUser.GroupId = groupId;
            await _dbContext.GroupUsers.AddAsync(groupUser);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveUserFromGroupAsync(string groupId, string userId)
        {
            if(!await _dbContext.Groups.AnyAsync(group => group.Id == groupId))
            {
                return false;
            }

            GroupUser? groupUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser => groupUser.UserId == userId);
            if (groupUser == null)
            {
                return false;
            }

            _dbContext.GroupUsers.Remove(groupUser);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<GroupUserStatus?> GetUserStatusAsync(string groupId, string userId)
        {
            GroupUser? groupUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser => groupUser.GroupId == groupId &&
                                                                                                groupUser.UserId == userId);
            return groupUser?.UserStatus;
        }


        #endregion
    }
}
