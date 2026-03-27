using System.ComponentModel.DataAnnotations;

namespace hms.Application.Configuration
{
    public class AppUrlConfiguration
    {
        [Required]
        public string ApiBaseUrl { get; set; } = string.Empty;
    }
}
