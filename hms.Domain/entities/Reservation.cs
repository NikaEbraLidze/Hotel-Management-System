namespace hms.Domain.Entities
{
    public class Reservation
    {
        // PK
        public Guid Id { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        // FK
        public Guid GuestId { get; set; }
        public Guest Guest { get; set; }

        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
    }
}
