namespace SocialConnect.Shared.Models
{
    public class EmailMessage
    {
        public string Subject { get; set; } = string.Empty;
        public string Reciever { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
