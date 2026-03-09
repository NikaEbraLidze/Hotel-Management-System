using hms.Application.Models.DTO;

namespace hms.Application.Contracts.Service
{
    public interface IReservationsService
    {
        Task<GetReservationResponseDTO> CreateReservationAsync(
            Guid hotelId,
            Guid guestId,
            CreateReservationRequestDTO request);
        Task<GetReservationResponseDTO> GetReservationByIdAsync(Guid reservationId, Guid guestId);
        Task<GetReservationResponseDTO> UpdateReservationAsync(
            Guid reservationId,
            Guid guestId,
            UpdateReservationRequestDTO request);
        Task DeleteReservationAsync(Guid reservationId, Guid guestId);
        Task<PagedResponseDTO<GetReservationResponseDTO>> GetHotelReservationsAsync(
            Guid hotelId,
            Guid currentUserId,
            bool isAdmin,
            GetHotelReservationsRequestDTO request);
        Task<List<GetRoomsResponseDTO>> GetAvailableRoomsAsync(
            Guid hotelId,
            GetAvailableReservationRoomsRequestDTO request);
    }
}
