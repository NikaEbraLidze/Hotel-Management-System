namespace hms.Application.Models.DTO
{
    public class UpdateHotelRequestDTO
    {
        public string? Name { get; set; }
        public byte? Rating { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}