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

        public Task<List<Room>> GetAvailableRoomsAsync(
            Guid hotelId,
            DateTime currentDate,
            DateTime? checkIn = null,
            DateTime? checkOut = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(room => room.HotelId == hotelId);

            if (checkIn.HasValue && checkOut.HasValue)
            {
                query = query.Where(room =>
                    !room.ReservationRooms.Any(reservationRoom =>
                        reservationRoom.Reservation.CheckInDate < checkOut.Value &&
                        checkIn.Value < reservationRoom.Reservation.CheckOutDate));
            }
            else
            {
                query = query.Where(room =>
                    !room.ReservationRooms.Any(reservationRoom =>
                        reservationRoom.Reservation.CheckInDate <= currentDate &&
                        currentDate < reservationRoom.Reservation.CheckOutDate));
            }

            return query
                .OrderBy(room => room.Name)
                .ToListAsync();
        }

        public Task<bool> HasActiveOrFutureReservationsAsync(Guid roomId, DateTime currentDate)
        {
            return _context.ReservationRooms.AnyAsync(
                reservationRoom => reservationRoom.RoomId == roomId &&
                                   reservationRoom.Reservation.CheckOutDate > currentDate);
        }
    }
}
