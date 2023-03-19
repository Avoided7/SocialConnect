using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Entitities;
using SocialConnect.WebUI.ViewModels;
using SocialConnect.WebUI.ViewModels.Enums;

namespace SocialConnect.WebUI.Extenstions
{
    public static class UserExtenstion
    {
        public static IEnumerable<UserVM> GetFriendsStatus(this IEnumerable<User> users, IEnumerable<FriendsCouple> friends)
        {
            return users.Select(user =>
            {
                UserVM userVM = new UserVM
                { 
                    Id = user.Id,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Username = user.UserName
                };
                if (friends.Any(friend => friend.FriendId == user.Id && friend.IsAgreed))
                {
                    userVM.Status = FriendStatus.Friend;
                }
                else if (friends.Any(friend => friend.FriendId == user.Id && !friend.IsAgreed))
                {
                    userVM.Status = FriendStatus.SendedRequest;
                }
                else if (friends.Any(friend => friend.UserId == user.Id && !friend.IsAgreed))
                {
                    userVM.Status = FriendStatus.WaitedResponse;
                }
                else
                {
                    userVM.Status = FriendStatus.Noname;
                }

                return userVM;
            });
        }
    }
}
