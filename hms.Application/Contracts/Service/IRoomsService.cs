using hms.Application.Models.DTO;

namespace hms.Application.Contracts.Service
{
    public interface IRoomsService
    {
        Task<PagedResponseDTO<GetRoomsResponseDTO>> GetRoomsAsync(Guid hotelId, GetRoomsRequestDTO request);
        Task<List<GetRoomsResponseDTO>> GetAvailableRoomsAsync(Guid hotelId, GetAvailableRoomsRequestDTO request);
        Task<GetRoomsResponseDTO> GetRoomByIdAsync(Guid hotelId, Guid roomId);
        Task<GetRoomsResponseDTO> CreateRoomAsync(Guid hotelId, CreateRoomRequestDTO request);
        Task<GetRoomsResponseDTO> UpdateRoomAsync(Guid hotelId, Guid roomId, UpdateRoomRequestDTO request);
        Task DeleteRoomAsync(Guid hotelId, Guid roomId);
    }
}
