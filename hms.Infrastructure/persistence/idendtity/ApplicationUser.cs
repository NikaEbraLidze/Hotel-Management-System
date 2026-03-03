using Microsoft.AspNetCore.Identity;

namespace hms.Infrastructure.Persistence.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalNumber { get; set; }
    }
}
