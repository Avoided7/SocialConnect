using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocialConnect.Infrastructure.Interfaces;
using SocialConnect.Shared.Models;

namespace SocialConnect.Infrastructure.Services
{
    public class MailKitEmailService : IMailKitEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailKitEmailService> _logger;

        public MailKitEmailService(IConfiguration configuration,
                            ILogger<MailKitEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task SendAsync(EmailMessage emailDto)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_configuration.GetSection("Mail:Sender").Value));
                message.To.Add(MailboxAddress.Parse(emailDto.Reciever));
                message.Subject = emailDto.Subject;
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = emailDto.Content
                };

                using (MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_configuration.GetSection("Mail:Username").Value,
                                                   _configuration.GetSection("Mail:Password").Value);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }
        }
    }
}
