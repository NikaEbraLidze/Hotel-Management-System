namespace hms.Application.Models.DTO
{
    public class GetGuestByIdResponseDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PersonalNumber { get; set; }
        public string PhoneNumber { get; set; }
    }
}
