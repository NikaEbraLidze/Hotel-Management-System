namespace hms.Domain.Entities
{

    public class Room
    {
        // PK
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        // FK
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }
        // Navigation property for M-M relationship
        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
    }
}