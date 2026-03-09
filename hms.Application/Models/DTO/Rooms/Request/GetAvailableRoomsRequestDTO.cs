namespace hms.Application.Models.DTO
{
    public class GetAvailableRoomsRequestDTO
    {
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
    }
}
