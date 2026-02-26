namespace hms.Domain.entities
{
    public class Guest
    {
        // PK
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalNumber { get; set; }
        public string PhoneNumber { get; set; }
    }
}