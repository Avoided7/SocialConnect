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
        IEnumerable<News> usersNews = await newsRepository.GetAsync(news => news.IsUserNews && users.Contains(news.UserId));
        IEnumerable<News> groupNews = await newsRepository.GetAsync(news => !news.IsUserNews && groups.Contains(news.GroupId));
        return usersNews.Concat(groupNews).OrderByDescending(news => news.WrittenIn);
    }
}