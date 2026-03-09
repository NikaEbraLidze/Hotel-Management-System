using hms.Domain.Entities;

namespace hms.Application.Contracts.Repository
{
    public interface IReservationsRepository : IRepositoryBase<Reservation>
    {
        Task<Reservation> GetByIdWithDetailsAsync(Guid reservationId, bool tracking = false);
        Task<(List<Reservation> Items, int TotalCount)> GetHotelReservationsAsync(
            Guid hotelId,
            DateTime? checkInFrom = null,
            DateTime? checkInTo = null,
            DateTime? checkOutFrom = null,
            DateTime? checkOutTo = null,
            int? pageNumber = null,
            int? pageSize = null,
            string orderBy = null,
            bool ascending = true,
            bool tracking = false);
        Task<List<Room>> GetHotelRoomsByIdsAsync(Guid hotelId, IEnumerable<Guid> roomIds, bool tracking = false);
        Task<List<Guid>> GetUnavailableRoomIdsAsync(
            Guid hotelId,
            IEnumerable<Guid> roomIds,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null);
        Task<List<Room>> GetAvailableRoomsAsync(
            Guid hotelId,
            DateTime currentDate,
            DateTime? checkInDate = null,
            DateTime? checkOutDate = null);
    }
}
