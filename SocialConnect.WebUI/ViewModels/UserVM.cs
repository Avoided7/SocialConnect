using SocialConnect.WebUI.ViewModels.Enums;

namespace SocialConnect.WebUI.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public FriendStatus Status { get; set; } = FriendStatus.Noname;
    }
}
