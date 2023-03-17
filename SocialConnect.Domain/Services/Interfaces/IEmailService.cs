using SocialConnect.Entity.Dtos;

namespace SocialConnect.Domain.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(EmailDto email);
    }
}
