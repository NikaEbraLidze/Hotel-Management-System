namespace hms.Application.Models.DTO
{
    public class GetHotelReservationsRequestDTO
    {
        public DateTime? CheckInFrom { get; set; }
        public DateTime? CheckInTo { get; set; }
        public DateTime? CheckOutFrom { get; set; }
        public DateTime? CheckOutTo { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string OrderBy { get; set; }
        public bool Ascending { get; set; } = true;
    }
}
