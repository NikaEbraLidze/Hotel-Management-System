namespace hms.Domain.Entities
{
    public class Hotel
    {
        // PK 
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte Rating { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        // Navigation properties  
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<HotelManager> HotelManagers { get; set; } = new List<HotelManager>();
    }
}
