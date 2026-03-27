using System.Net;
using System.Net.Mail;
using hms.Application.Configuration;
using hms.Application.Contracts.Service;
using Microsoft.Extensions.Options;

namespace hms.Application.Services
{
    public class EmailService : IEmailService, IDisposable
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly SmtpClient _smtpClient;
        private bool _disposed;

        public EmailService(IOptions<EmailConfiguration> emailConfig)
        {
            _emailConfig = emailConfig.Value;
            _smtpClient = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password),
                EnableSsl = _emailConfig.EnableSsl
            };
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = false)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailConfig.From, _emailConfig.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };

            mailMessage.To.Add(to);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _smtpClient.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
