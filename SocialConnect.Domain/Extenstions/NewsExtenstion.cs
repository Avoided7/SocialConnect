using System.Collections;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.Domain.Extenstions;

public static class NewsExtenstion
{
    public static async Task<bool> HasEditingRightsAsync(this INewsRepository newsRepository, string newsId, string userId)
    {
        News? news = await newsRepository.FirstOrDefaultAsync(news => news.Id == newsId);
        if (news == null)
        {
            return false;
        }

        if (news.GroupId == null)
        {
            return news.UserId == userId;
        }

        GroupUser? groupUser = news.Group?.Users.FirstOrDefault(groupUser => groupUser.UserId == userId);
        if (groupUser == null || groupUser.UserStatus == GroupUserStatus.User)
        {
            return false;
        }

        return true;
    }
    public static async Task<IReadOnlyCollection<News>> GetNewsFromUsersNGroupsAsync(this INewsRepository newsRepository,
                                                                 IEnumerable<string> users,
                                                                 IEnumerable<string> groups)
    {
        IReadOnlyCollection<News> usersNews = (await newsRepository.GetAsync(news => news.GroupId == null &&
            users.Contains(news.UserId))).ToList();
        
        IReadOnlyCollection<News> groupNews = (await newsRepository.GetAsync(news => news.GroupId != null &&
            groups.Contains(news.GroupId))).ToList();
        
        return usersNews.Concat(groupNews).OrderByDescending(news => news.WrittenIn).ToList();
    }
}