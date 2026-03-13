using hms.Application.Models.DTO;
using Microsoft.AspNetCore.Http;

namespace hms.Application.Contracts.Service
{
    public interface IHotelService
    {
        Task<PagedResponseDTO<GetHotelsResponseDTO>> GetHotelsAsync(GetHotelsRequestDTO request);
        Task<GetHotelByIdResponseDTO> GetHotelByIdAsync(Guid request);
        Task<RegisterHotelResponseDTO> RegisterHotelAsync(RegisterHotelRequestDTO request, IFormFile image);
        Task<UpdateHotelResponseDTO> UpdateHotelAsync(Guid id, UpdateHotelRequestDTO request, IFormFile image);
        Task DeleteHotelAsync(Guid request);
    }
}
