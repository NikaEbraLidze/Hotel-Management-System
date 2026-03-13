namespace hms.Application.Models.DTO
{
    public class GetHotelsResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte Rating { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ImageUrl { get; set; }
        public string ImagePublicId { get; set; }
    }
}
