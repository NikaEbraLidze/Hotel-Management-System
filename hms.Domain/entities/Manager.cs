namespace hms.Domain.entities
{
    public class Manager
    {
        // PK
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        // FK
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }

    }
}