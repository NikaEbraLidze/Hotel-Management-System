using Microsoft.AspNetCore.Identity;
using hms.Domain.Entities;

namespace hms.Domain.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalNumber { get; set; }
        public ICollection<HotelManager> ManagedHotels { get; set; } = new List<HotelManager>();
    }
}
