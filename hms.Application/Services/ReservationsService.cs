using hms.Application.Contracts.Repository;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;
using hms.Domain.Entities;
using MapsterMapper;

namespace hms.Application.Services
{
    public class ReservationsService : IReservationsService
    {
        private readonly IReservationsRepository _reservationsRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IHotelManagersRepository _hotelManagersRepository;
        private readonly IGuestsRepository _guestsRepository;
        private readonly IMapper _mapper;

        public ReservationsService(
            IReservationsRepository reservationsRepository,
            IHotelRepository hotelRepository,
            IHotelManagersRepository hotelManagersRepository,
            IGuestsRepository guestsRepository,
            IMapper mapper)
        {
            _reservationsRepository = reservationsRepository;
            _hotelRepository = hotelRepository;
            _hotelManagersRepository = hotelManagersRepository;
            _guestsRepository = guestsRepository;
            _mapper = mapper;
        }

        public async Task<GetReservationResponseDTO> CreateReservationAsync(
            Guid hotelId,
            Guid guestId,
            CreateReservationRequestDTO request)
        {
            ReservationsValidation.ValidateCreateReservationRequest(hotelId, request);

            await EnsureHotelExistsAsync(hotelId);
            await EnsureGuestExistsAsync(guestId);

            var roomIds = request.RoomIds.Distinct().ToList();
            var checkInDate = request.CheckInDate.Date;
            var checkOutDate = request.CheckOutDate.Date;

            await EnsureRoomsExistForHotelAsync(hotelId, roomIds);
            await EnsureRoomsAvailableAsync(hotelId, roomIds, checkInDate, checkOutDate);

            var reservation = _mapper.Map<Reservation>(request);
            reservation.GuestId = guestId;
            reservation.ReservationRooms = roomIds
                .Select(roomId => new ReservationRoom { RoomId = roomId })
                .ToList();

            await _reservationsRepository.AddAsync(reservation);
            await _reservationsRepository.SaveAsync();

            var createdReservation = await _reservationsRepository.GetByIdWithDetailsAsync(reservation.Id)
                ?? throw new NotFoundException($"Reservation with ID {reservation.Id} not found.");

            return _mapper.Map<GetReservationResponseDTO>(createdReservation);
        }

        public async Task<GetReservationResponseDTO> GetReservationByIdAsync(Guid reservationId, Guid guestId)
        {
            ReservationsValidation.ValidateReservationId(reservationId);

            await EnsureGuestExistsAsync(guestId);

            var reservation = await GetOwnedReservationAsync(reservationId, guestId, tracking: false);
            return _mapper.Map<GetReservationResponseDTO>(reservation);
        }

        public async Task<GetReservationResponseDTO> UpdateReservationAsync(
            Guid reservationId,
            Guid guestId,
            UpdateReservationRequestDTO request)
        {
            ReservationsValidation.ValidateUpdateReservationRequest(reservationId, request);

            await EnsureGuestExistsAsync(guestId);

            var reservation = await GetOwnedReservationAsync(reservationId, guestId, tracking: true);
            EnsureReservationCanBeModified(reservation);

            var hotelId = GetReservationHotelId(reservation);
            var roomIds = reservation.ReservationRooms
                .Select(reservationRoom => reservationRoom.RoomId)
                .Distinct()
                .ToList();

            await EnsureRoomsAvailableAsync(
                hotelId,
                roomIds,
                request.CheckInDate.Date,
                request.CheckOutDate.Date,
                reservation.Id);

            _mapper.Map(request, reservation);

            _reservationsRepository.UpdateAsync(reservation);
            await _reservationsRepository.SaveAsync();

            var updatedReservation = await _reservationsRepository.GetByIdWithDetailsAsync(reservation.Id)
                ?? throw new NotFoundException($"Reservation with ID {reservation.Id} not found.");

            return _mapper.Map<GetReservationResponseDTO>(updatedReservation);
        }

        public async Task DeleteReservationAsync(Guid reservationId, Guid guestId)
        {
            ReservationsValidation.ValidateReservationId(reservationId);

            await EnsureGuestExistsAsync(guestId);

            var reservation = await GetOwnedReservationAsync(reservationId, guestId, tracking: true);
            EnsureReservationCanBeModified(reservation);

            _reservationsRepository.DeleteAsync(reservation);
            await _reservationsRepository.SaveAsync();
        }

        public async Task<PagedResponseDTO<GetReservationResponseDTO>> GetHotelReservationsAsync(
            Guid hotelId,
            Guid currentUserId,
            bool isAdmin,
            GetHotelReservationsRequestDTO request)
        {
            ReservationsValidation.ValidateGetHotelReservationsRequest(hotelId, request);

            await EnsureHotelExistsAsync(hotelId);

            if (!isAdmin)
                await EnsureManagerHasAccessToHotelAsync(hotelId, currentUserId);

            var reservations = await _reservationsRepository.GetHotelReservationsAsync(
                hotelId,
                request.CheckInFrom?.Date,
                request.CheckInTo?.Date,
                request.CheckOutFrom?.Date,
                request.CheckOutTo?.Date,
                request.PageNumber,
                request.PageSize,
                Normalize(request.OrderBy),
                request.Ascending,
                tracking: false);

            return new PagedResponseDTO<GetReservationResponseDTO>
            {
                Items = _mapper.Map<List<GetReservationResponseDTO>>(reservations.Items),
                TotalCount = reservations.TotalCount,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? reservations.Items.Count
            };
        }

        public async Task<List<GetRoomsResponseDTO>> GetAvailableRoomsAsync(
            Guid hotelId,
            GetAvailableReservationRoomsRequestDTO request)
        {
            ReservationsValidation.ValidateGetAvailableRoomsRequest(hotelId, request);

            await EnsureHotelExistsAsync(hotelId);

            var rooms = await _reservationsRepository.GetAvailableRoomsAsync(
                hotelId,
                DateTime.UtcNow,
                request.CheckInDate?.Date,
                request.CheckOutDate?.Date);

            return _mapper.Map<List<GetRoomsResponseDTO>>(rooms);
        }

        private async Task<Reservation> GetOwnedReservationAsync(Guid reservationId, Guid guestId, bool tracking)
        {
            var reservation = await _reservationsRepository.GetByIdWithDetailsAsync(reservationId, tracking)
                ?? throw new NotFoundException($"Reservation with ID {reservationId} not found.");

            if (reservation.GuestId != guestId)
                throw new ForbiddenException("You do not have access to this reservation.");

            return reservation;
        }

        private async Task EnsureHotelExistsAsync(Guid hotelId)
        {
            var hotelExists = await _hotelRepository.ExistsAsync(hotel => hotel.Id == hotelId);

            if (!hotelExists)
                throw new NotFoundException($"Hotel with ID {hotelId} not found.");
        }

        private async Task EnsureGuestExistsAsync(Guid guestId)
        {
            var guest = await _guestsRepository.GetByIdAsync(guestId);

            if (guest is null)
                throw new UnauthorizedAccessException("Authenticated guest account was not found.");
        }

        private async Task EnsureManagerHasAccessToHotelAsync(Guid hotelId, Guid managerId)
        {
            var managesHotel = await _hotelManagersRepository.ExistsAsync(hotelId, managerId);

            if (!managesHotel)
                throw new ForbiddenException($"You do not have access to hotel {hotelId} reservations.");
        }

        private async Task EnsureRoomsExistForHotelAsync(Guid hotelId, List<Guid> roomIds)
        {
            var rooms = await _reservationsRepository.GetHotelRoomsByIdsAsync(hotelId, roomIds, tracking: false);

            if (rooms.Count == roomIds.Count)
                return;

            var missingRoomIds = roomIds.Except(rooms.Select(room => room.Id)).ToList();

            throw new NotFoundException(
                $"Rooms with IDs {string.Join(", ", missingRoomIds)} were not found for hotel {hotelId}.");
        }

        private async Task EnsureRoomsAvailableAsync(
            Guid hotelId,
            List<Guid> roomIds,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null)
        {
            var unavailableRoomIds = await _reservationsRepository.GetUnavailableRoomIdsAsync(
                hotelId,
                roomIds,
                checkInDate,
                checkOutDate,
                excludeReservationId);

            if (unavailableRoomIds.Count == 0)
                return;

            throw new ConflictException(
                $"Rooms with IDs {string.Join(", ", unavailableRoomIds)} are not available for the selected date range.");
        }

        private static void EnsureReservationCanBeModified(Reservation reservation)
        {
            var today = DateTime.UtcNow.Date;

            if (reservation.CheckInDate.Date <= today)
                throw new ConflictException("Only future reservations can be updated or cancelled.");
        }

        private static Guid GetReservationHotelId(Reservation reservation)
        {
            var hotelId = reservation.ReservationRooms
                .Select(reservationRoom => reservationRoom.Room.HotelId)
                .FirstOrDefault();

            if (hotelId == Guid.Empty)
                throw new NotFoundException($"Reservation with ID {reservation.Id} has no linked hotel.");

            return hotelId;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
