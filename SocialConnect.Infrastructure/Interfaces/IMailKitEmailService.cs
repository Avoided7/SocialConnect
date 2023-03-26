using SocialConnect.Shared.Models;

namespace SocialConnect.Infrastructure.Interfaces
{
    public interface IMailKitEmailService
    {
        void SendAsync(EmailMessage email);
    }
}
