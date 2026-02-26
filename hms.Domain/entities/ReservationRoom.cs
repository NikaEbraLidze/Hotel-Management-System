namespace hms.Domain.entities
{
    public class ReservationRoom
    {
        // Composite PK
        public Guid ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
    }
}