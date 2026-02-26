namespace hms.Domain.entities
{
    public class Hotel
    {
        // PK 
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte Rating { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        // Navigation properties  
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Manager> Managers { get; set; } = new List<Manager>();
    }
}