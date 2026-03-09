using hms.Application.Contracts.Repository;
using hms.Domain.Entities;
using hms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace hms.Infrastructure.Repository
{
    public class ReservationsRepository : RepositoryBase<Reservation>, IReservationsRepository
    {
        public ReservationsRepository(HmsDbContext context) : base(context)
        {
        }

        public async Task<Reservation> GetByIdWithDetailsAsync(Guid reservationId, bool tracking = false)
        {
            IQueryable<Reservation> query = _dbSet
                .Include(reservation => reservation.Guest)
                .Include(reservation => reservation.ReservationRooms)
                    .ThenInclude(reservationRoom => reservationRoom.Room)
                .Where(reservation => reservation.Id == reservationId)
                .AsSplitQuery();

            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Reservation>> GetGuestReservationsAsync(Guid guestId, bool tracking = false)
        {
            IQueryable<Reservation> query = _dbSet
                .Include(reservation => reservation.Guest)
                .Include(reservation => reservation.ReservationRooms)
                    .ThenInclude(reservationRoom => reservationRoom.Room)
                .Where(reservation => reservation.GuestId == guestId)
                .OrderBy(reservation => reservation.CheckInDate)
                .ThenBy(reservation => reservation.Id)
                .AsSplitQuery();

            if (!tracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<(List<Reservation> Items, int TotalCount)> GetHotelReservationsAsync(
            Guid hotelId,
            DateTime? checkInFrom = null,
            DateTime? checkInTo = null,
            DateTime? checkOutFrom = null,
            DateTime? checkOutTo = null,
            int? pageNumber = null,
            int? pageSize = null,
            string orderBy = null,
            bool ascending = true,
            bool tracking = false)
        {
            IQueryable<Reservation> query = _dbSet
                .Where(reservation => reservation.ReservationRooms.Any(reservationRoom => reservationRoom.Room.HotelId == hotelId));

            if (!tracking)
                query = query.AsNoTracking();

            if (checkInFrom.HasValue)
                query = query.Where(reservation => reservation.CheckInDate >= checkInFrom.Value);

            if (checkInTo.HasValue)
                query = query.Where(reservation => reservation.CheckInDate <= checkInTo.Value);

            if (checkOutFrom.HasValue)
                query = query.Where(reservation => reservation.CheckOutDate >= checkOutFrom.Value);

            if (checkOutTo.HasValue)
                query = query.Where(reservation => reservation.CheckOutDate <= checkOutTo.Value);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, orderBy, ascending);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                var skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }

            query = query
                .Include(reservation => reservation.Guest)
                .Include(reservation => reservation.ReservationRooms)
                    .ThenInclude(reservationRoom => reservationRoom.Room)
                .AsSplitQuery();

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        public async Task<List<Room>> GetHotelRoomsByIdsAsync(Guid hotelId, IEnumerable<Guid> roomIds, bool tracking = false)
        {
            var normalizedRoomIds = roomIds
                .Distinct()
                .ToList();

            IQueryable<Room> query = _context.Rooms
                .Where(room => room.HotelId == hotelId && normalizedRoomIds.Contains(room.Id));

            if (!tracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<List<Guid>> GetUnavailableRoomIdsAsync(
            Guid hotelId,
            IEnumerable<Guid> roomIds,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null)
        {
            var normalizedRoomIds = roomIds
                .Distinct()
                .ToList();

            if (normalizedRoomIds.Count == 0)
                return new List<Guid>();

            var query = _context.ReservationRooms
                .AsNoTracking()
                .Where(reservationRoom =>
                    normalizedRoomIds.Contains(reservationRoom.RoomId) &&
                    reservationRoom.Room.HotelId == hotelId &&
                    reservationRoom.Reservation.CheckInDate < checkOutDate &&
                    checkInDate < reservationRoom.Reservation.CheckOutDate);

            if (excludeReservationId.HasValue)
                query = query.Where(reservationRoom => reservationRoom.ReservationId != excludeReservationId.Value);

            return await query
                .Select(reservationRoom => reservationRoom.RoomId)
                .Distinct()
                .ToListAsync();
        }

        public Task<List<Room>> GetAvailableRoomsAsync(
            Guid hotelId,
            DateTime currentDate,
            DateTime? checkInDate = null,
            DateTime? checkOutDate = null)
        {
            var query = _context.Rooms
                .AsNoTracking()
                .Where(room => room.HotelId == hotelId);

            if (checkInDate.HasValue && checkOutDate.HasValue)
            {
                query = query.Where(room =>
                    !room.ReservationRooms.Any(reservationRoom =>
                        reservationRoom.Reservation.CheckInDate < checkOutDate.Value &&
                        checkInDate.Value < reservationRoom.Reservation.CheckOutDate));
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

        private static IQueryable<Reservation> ApplySorting(
            IQueryable<Reservation> query,
            string orderBy,
            bool ascending)
        {
            var normalizedOrderBy = string.IsNullOrWhiteSpace(orderBy)
                ? nameof(Reservation.CheckInDate)
                : orderBy.Trim();

            var orderByCheckOutDate = normalizedOrderBy.Equals(
                nameof(Reservation.CheckOutDate),
                StringComparison.OrdinalIgnoreCase);

            if (orderByCheckOutDate)
                return ascending
                    ? query.OrderBy(reservation => reservation.CheckOutDate).ThenBy(reservation => reservation.Id)
                    : query.OrderByDescending(reservation => reservation.CheckOutDate).ThenByDescending(reservation => reservation.Id);

            return ascending
                ? query.OrderBy(reservation => reservation.CheckInDate).ThenBy(reservation => reservation.Id)
                : query.OrderByDescending(reservation => reservation.CheckInDate).ThenByDescending(reservation => reservation.Id);
        }
    }
}
