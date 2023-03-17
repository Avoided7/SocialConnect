using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SocialConnect.Domain.Services.Interfaces;
using SocialConnect.Entity.Dtos;

namespace SocialConnect.Domain.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration,
                            ILogger<EmailService> logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }
        public async Task<bool> SendAsync(EmailDto emailDto)
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

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return false;
            }
        }
    }
}
