using System.Collections;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.Domain.Extenstions;

public static class NewsExtenstion
{
    public static async Task<IEnumerable<News>> GetNewsFromUsersNGroupsAsync(this INewsRepository newsRepository,
                                                                 IEnumerable<string> users,
                                                                 IEnumerable<string> groups)
    {
        IReadOnlyCollection<News> usersNews = (await newsRepository.GetAsync(news => news.GroupId == null &&
            users.Contains(news.UserId))).ToList();
        
        IReadOnlyCollection<News> groupNews = (await newsRepository.GetAsync(news => news.GroupId != null &&
            groups.Contains(news.GroupId))).ToList();
        
        return usersNews.Concat(groupNews).OrderByDescending(news => news.WrittenIn);
    }
}