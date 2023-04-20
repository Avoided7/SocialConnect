using SocialConnect.Domain.Entities;
using SocialConnect.WebUI.Enums;
using SocialConnect.WebUI.ViewModels;
using System.Security.Claims;

namespace SocialConnect.WebUI.Extenstions
{
    public static class UserExtenstion
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        public static IEnumerable<UserVM> GetFriendsStatus(this IEnumerable<User> users, IEnumerable<FriendsCouple> friends)
        {
            IReadOnlyList<FriendsCouple> friendsList = friends.ToList();
            return users.Select(user =>
            {
                UserVM userVm = new UserVM
                { 
                    Id = user.Id,
                    OnlineStatus = user.Status.IsOnline,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Username = user.UserName
                };
                if (friendsList.Any(friend => friend.FriendId == user.Id && friend.IsAgreed))
                {
                    userVm.Status = FriendStatus.Friend;
                }
                else if (friendsList.Any(friend => friend.FriendId == user.Id && !friend.IsAgreed))
                {
                    userVm.Status = FriendStatus.SendedRequest;
                }
                else if (friendsList.Any(friend => friend.UserId == user.Id && !friend.IsAgreed))
                {
                    userVm.Status = FriendStatus.WaitedResponse;
                }
                else
                {
                    userVm.Status = FriendStatus.Noname;
                }

                return userVm;
            });
        }
    }
}
