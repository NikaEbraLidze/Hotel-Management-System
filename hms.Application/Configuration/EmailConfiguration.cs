using System.ComponentModel.DataAnnotations;

namespace hms.Application.Configuration
{
    public class EmailConfiguration
    {
        [Required]
        [EmailAddress]
        public string From { get; set; } = string.Empty;

        public string FromName { get; set; } = string.Empty;

        [Required]
        public string SmtpServer { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; }

        public bool EnableSsl { get; set; } = true;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
