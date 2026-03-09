using hms.Domain.Entities;

namespace hms.Application.Contracts.Repository
{
    public interface IRoomsRepository : IRepositoryBase<Room>
    {
        Task<List<Room>> GetAvailableRoomsAsync(Guid hotelId, DateTime currentDate, DateTime? checkIn = null, DateTime? checkOut = null);
        Task<bool> HasActiveOrFutureReservationsAsync(Guid roomId, DateTime currentDate);
    }
}
