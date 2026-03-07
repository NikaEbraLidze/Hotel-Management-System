using hms.Application.Models.DTO;

namespace hms.Application.Contracts.Service
{
    public interface IHotelService
    {
        Task<PagedResponseDTO<GetHotelsResponseDTO>> GetHotelsAsync(GetHotelsRequestDTO request);
        Task<RegisterHotelResponseDTO> RegisterHotelAsync(RegisterHotelRequestDTO request);
    }
}