namespace hms.Application.Models.DTO
{
    public class GetHotelsRequestDTO
    {
        public string? Name { get; set; }
        public byte? Rating { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        public string? OrderBy { get; set; }
        public bool Ascending { get; set; }
    }
}