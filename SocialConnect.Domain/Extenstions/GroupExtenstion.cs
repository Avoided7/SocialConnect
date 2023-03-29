using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Interfaces;
using System.Diagnostics.Metrics;

namespace SocialConnect.Domain.Extenstions
{
    public static class GroupExtenstion
    {
        public static async Task<bool> IsFounderAsync(this IGroupRepository groupRepository, string groupId, string userId)
        {
            return (await groupRepository
                                        .FirstOrDefaultAsync(group => group.Id == groupId))
                                        ?.Users
                                        .FirstOrDefault(user => user.Id == userId)
                                        ?.UserStatus == GroupUserStatus.Founder;
        }
        public static async Task<int> GetGroupsRequestsCountAsync(this IGroupRepository groupRepository, string userId)
        {
            IEnumerable<Group> groupRequests = await groupRepository.GetAsync(group => group.Users.Any(groupUser => groupUser.UserId == userId &&
                                                                                                                            groupUser.UserStatus != GroupUserStatus.User));
            int count = groupRequests.Sum(group => group.Users.Count(user => user.IsAgreed == false));

            return count;
        }
        public static async Task<bool> JoinUserAsync(this IGroupRepository groupRepository, string groupId, string userId)
        {
            Group? group = await groupRepository.FirstOrDefaultAsync(group => group.Id == groupId);
            if (group == null)
            {
                return false;
            }

            if (group.Users.Any(user => user.UserId == userId))
            {
                return false;
            }

            GroupUser newUser = new GroupUser
            {
                GroupId = groupId,
                UserId = userId,
                UserStatus = GroupUserStatus.User
            };

            if (group.UserCount == 0)
            {
                newUser.UserStatus = GroupUserStatus.Founder;
                newUser.IsAgreed = true;
            }

            return await groupRepository.AddUserToGroupAsync(groupId, newUser); 
        }
        public static async Task<bool> LeftUserAsync(this IGroupRepository groupRepository,  string groupId, string userId)
        {
            Group? group = await groupRepository.FirstOrDefaultAsync(group => group.Id == groupId);
            if (group == null)
            {
                return false;
            }
            if (group.UserCount == 1)
            {
                return await groupRepository.DeleteAsync(group.Id);
            }
            GroupUser? existedGroupUser = group.Users.FirstOrDefault(groupUser => groupUser.GroupId == groupId &&
                                                                                    groupUser.UserId == userId);
            if (existedGroupUser == null || existedGroupUser.UserStatus == GroupUserStatus.Founder)
            {
                return false;
            }
                
            return await groupRepository.RemoveUserFromGroupAsync(groupId, userId);
        }
        public static async Task<bool> PromoteUserAsync(this IGroupRepository groupRepository, string currentUserId, string userId, string groupId, GroupUserStatus newStatus)
        {
            if (newStatus == GroupUserStatus.Founder)
            {
                return false;
            }

            Group? group = await groupRepository.FirstOrDefaultAsync(group => group.Id == groupId);
            if (group == null)
            {
                return false;
            }

            GroupUser? promotedUser = group.Users.FirstOrDefault(groupUser => groupUser.UserId == userId);
            if (promotedUser == null || promotedUser.UserStatus == GroupUserStatus.Founder)
            {
                return false;
            }

            if (!group.Users.Any(groupUser => groupUser.UserId == currentUserId && groupUser.UserStatus == GroupUserStatus.Founder))
            {
                return false;
            }

            promotedUser.UserStatus = newStatus;

            return await groupRepository.UpdateGroupUserAsync(groupId, userId, promotedUser) != null;
        }
        public static async Task<bool> AcceptUserAsync(this IGroupRepository groupRepository, string currenUserId, string userId, string groupId)
        {
            Group? group = await groupRepository.FirstOrDefaultAsync(group => group.Id == groupId);

            if (group == null)
            {
                return false;
            }

            if (!group.Users.Any(user => user.UserId == currenUserId && user.UserStatus != GroupUserStatus.User))
            {
                return false;
            }

            GroupUser? acceptedUser = group.Users.FirstOrDefault(user => user.UserId == userId);

            if (acceptedUser == null || acceptedUser.IsAgreed)
            {
                return false;
            }

            acceptedUser.IsAgreed = true;

                

            return groupRepository.UpdateGroupUserAsync(groupId, userId, acceptedUser) != null;
        }
        public static async Task<bool> DeclineRequestAsync(this IGroupRepository groupRepository, string userId, string groupId)
        {
            return await groupRepository.RemoveUserFromGroupAsync(groupId, userId);
        }
        public static async Task<bool> DeclineRequestAsync(this IGroupRepository groupRepository, string currentUserId, string userId, string groupId)
        {
            GroupUserStatus? currentUser = await groupRepository.GetUserStatusAsync(groupId, currentUserId);
            if (currentUser == null || currentUser == GroupUserStatus.User)
            {
                return false;
            }

            return await groupRepository.RemoveUserFromGroupAsync(groupId, userId);
        }
        public static async Task<bool> KickUserAsync(this IGroupRepository groupRepository, string currentUserId, string userId, string groupId)
        {
            Group? group = await groupRepository.FirstOrDefaultAsync(group => group.Id == groupId);

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

            return await groupRepository.RemoveUserFromGroupAsync(groupId, kickedUser.Id);
        }
    }
}
