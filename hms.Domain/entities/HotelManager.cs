using hms.Domain.Identity;

namespace hms.Domain.Entities
{
    public class HotelManager
    {
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public Guid ManagerUserId { get; set; }
        public ApplicationUser ManagerUser { get; set; }
    }
}
