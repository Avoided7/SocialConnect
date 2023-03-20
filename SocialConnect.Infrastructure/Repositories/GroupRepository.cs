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

        public GroupRepository(SocialDbContext dbContext,
                               ILogger<GroupRepository> logger)
        {
            this._dbContext = dbContext;
            this._logger = logger;
        }
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

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                Group? group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id);
                if(group == null)
                {
                    return false;
                }

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

        public async Task<Group?> FirstOrDefaultAsync(Expression<Func<Group, bool>> expression)
        {
            try
            {
                return await _dbContext.Groups
                                              .Include(group => group.Users)
                                              .ThenInclude(groupUser => groupUser.User)
                                              .FirstOrDefaultAsync(expression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<Group>> GetAsync()
        {
            try
            {
                return await _dbContext.Groups
                                              .Include(group => group.Users)
                                              .ThenInclude(groupUser => groupUser.User)
                                              .ToListAsync();
                                              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Enumerable.Empty<Group>();
            }
        }

        public async Task<IEnumerable<Group>> GetAsync(Expression<Func<Group, bool>> expression)
        {
            try
            {
                return await _dbContext.Groups
                                              .Include(group => group.Users)
                                              .ThenInclude(groupUser => groupUser.User)
                                              .Where(expression)
                                              .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Enumerable.Empty<Group>() ;
            }
        }

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
                if (existedGroupUser == null)
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

        public async Task<bool> PromoteUserAsync(GroupUser promoteUser)
        {
            try
            {
                if(promoteUser.UserStatus == GroupUserStatus.Founder)
                {
                    return false;
                }
                Group? group = await FirstOrDefaultAsync(group => group.Id == promoteUser.GroupId);
                if (group == null)
                {
                    return false;
                }
                GroupUser? existedGroupUser = await _dbContext.GroupUsers.FirstOrDefaultAsync(groupUser => groupUser.GroupId == promoteUser.GroupId &&
                                                                                                           groupUser.UserId == promoteUser.UserId);
                if (existedGroupUser == null)
                {
                    return false;
                }

                if(existedGroupUser.UserStatus == GroupUserStatus.Founder)
                {
                    return false;
                }
                existedGroupUser.UserStatus = promoteUser.UserStatus;
                _dbContext.GroupUsers.Update(existedGroupUser);
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
}
