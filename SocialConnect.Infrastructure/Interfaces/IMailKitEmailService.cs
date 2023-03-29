using SocialConnect.Shared.Models;

namespace SocialConnect.Infrastructure.Interfaces
{
    public interface IMailKitEmailService
    {
        Task SendAsync(EmailMessage email);
    }
}
