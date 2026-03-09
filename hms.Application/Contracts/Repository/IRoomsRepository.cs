using hms.Domain.Entities;

namespace hms.Application.Contracts.Repository
{
    public interface IRoomsRepository : IRepositoryBase<Room>
    {
        Task<bool> HasActiveOrFutureReservationsAsync(Guid roomId, DateTime currentDate);
    }
}
