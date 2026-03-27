namespace hms.Application.Contracts.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = false);
    }
}
