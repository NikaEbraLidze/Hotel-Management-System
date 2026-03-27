using System.Net;
using System.Net.Mail;
using hms.Application.Configuration;
using hms.Application.Contracts.Service;
using Microsoft.Extensions.Options;

namespace hms.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        public EmailService(IOptions<EmailConfiguration> emailConfig)
        {
            _emailConfig = emailConfig.Value;
        }

        public Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = false)
        {
            using var client = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port)
            {
                Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password),
                EnableSsl = _emailConfig.EnableSsl,
                UseDefaultCredentials = false
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailConfig.From, _emailConfig.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };

            mailMessage.To.Add(to);

            return client.SendMailAsync(mailMessage);
        }
    }
}
