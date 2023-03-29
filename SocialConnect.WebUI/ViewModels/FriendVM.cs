namespace SocialConnect.WebUI.ViewModels
{
    public class FriendVM
    {
        public bool HasAny => Friends.Any() || SendedRequest.Any() || WaitedResponse.Any();
        public IList<UserVM> Friends { get; set; } = null!;
        public IList<UserVM> SendedRequest { get; set; } = null!;
        public IList<UserVM> WaitedResponse { get; set; } = null!;
    }
}
