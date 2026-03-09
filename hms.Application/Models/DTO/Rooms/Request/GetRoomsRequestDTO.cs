namespace hms.Application.Models.DTO
{
    public class GetRoomsRequestDTO
    {
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        public string OrderBy { get; set; }
        public bool Ascending { get; set; }
    }
}
