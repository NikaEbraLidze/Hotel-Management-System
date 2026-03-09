namespace hms.Application.Models.DTO
{
    public class GetAvailableReservationRoomsRequestDTO
    {
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
    }
}
