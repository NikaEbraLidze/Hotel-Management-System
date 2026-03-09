using hms.Application.Models.DTO;

namespace hms.Application.Contracts.Service
{
    public interface IHotelManagersService
    {
        Task<AddHotelManagerResponseDTO> AddHotelManagerAsync(Guid hotelId, AddHotelManagerRequestDTO request);
        Task<List<GetHotelManagersResponseDTO>> GetHotelManagersAsync(Guid hotelId);
        Task<GetHotelManagerByIdResponseDTO> GetHotelManagerByIdAsync(Guid hotelId, Guid managerId);
        Task<UpdateHotelManagerResponseDTO> UpdateHotelManagerAsync(Guid hotelId, Guid managerId, UpdateHotelManagerRequestDTO request);
        Task DeleteHotelManagerAsync(Guid hotelId, Guid managerId);
    }
}
