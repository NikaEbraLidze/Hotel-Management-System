using hms.Application.Models.DTO;

namespace hms.Application.Contracts.Service
{
    public interface IGuestsService
    {
        Task<List<GetGuestsResponseDTO>> GetGuestsAsync();
        Task<GetGuestByIdResponseDTO> GetGuestByIdAsync(Guid guestId);
        Task<UpdateGuestResponseDTO> UpdateGuestAsync(Guid guestId, UpdateGuestRequestDTO request);
        Task DeleteGuestAsync(Guid guestId);
    }
}
