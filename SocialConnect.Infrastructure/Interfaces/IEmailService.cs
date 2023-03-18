using SocialConnect.Shared.Models;

namespace SocialConnect.Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(EmailMessage email);
    }
}
