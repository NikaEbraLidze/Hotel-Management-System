namespace hms.Application.Models.DTO
{
    public class CreateReservationRequestDTO
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public List<Guid> RoomIds { get; set; } = new();
    }
}
