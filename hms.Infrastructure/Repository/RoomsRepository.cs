using hms.Application.Contracts.Repository;
using hms.Domain.Entities;
using hms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace hms.Infrastructure.Repository
{
    public class RoomsRepository : RepositoryBase<Room>, IRoomsRepository
    {
        public RoomsRepository(HmsDbContext context) : base(context)
        {
        }

        public Task<bool> HasActiveOrFutureReservationsAsync(Guid roomId, DateTime currentDate)
        {
            return _context.ReservationRooms.AnyAsync(
                reservationRoom => reservationRoom.RoomId == roomId &&
                                   reservationRoom.Reservation.CheckOutDate > currentDate);
        }
    }
}
