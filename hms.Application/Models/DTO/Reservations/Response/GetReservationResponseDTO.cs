namespace hms.Application.Models.DTO
{
    public class GetReservationResponseDTO
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public ReservationGuestResponseDTO Guest { get; set; }
        public List<ReservationRoomResponseDTO> Rooms { get; set; } = new();
    }
}
