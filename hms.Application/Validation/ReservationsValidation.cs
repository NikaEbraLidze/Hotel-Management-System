using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Domain.Entities;

namespace hms.Application.Validation
{
    public static class ReservationsValidation
    {
        private static readonly HashSet<string> AllowedOrderByFields = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Reservation.CheckInDate),
            nameof(Reservation.CheckOutDate),
        };

        public static void ValidateHotelId(Guid hotelId)
        {
            ValidateGuid(hotelId, "Hotel ID");
        }

        public static void ValidateReservationId(Guid reservationId)
        {
            ValidateGuid(reservationId, "Reservation ID");
        }

        public static void ValidateCreateReservationRequest(Guid hotelId, CreateReservationRequestDTO request)
        {
            ValidateHotelId(hotelId);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            ValidateReservationDates(request.CheckInDate, request.CheckOutDate);
            ValidateRoomIds(request.RoomIds);
        }

        public static void ValidateUpdateReservationRequest(Guid reservationId, UpdateReservationRequestDTO request)
        {
            ValidateReservationId(reservationId);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            ValidateReservationDates(request.CheckInDate, request.CheckOutDate);
        }

        public static void ValidateGetHotelReservationsRequest(Guid hotelId, GetHotelReservationsRequestDTO request)
        {
            ValidateHotelId(hotelId);

            if (request is null)
                throw new BadRequestException("Request query is required.");

            ValidateDateFilterRange(request.CheckInFrom, request.CheckInTo, "Check-in");
            ValidateDateFilterRange(request.CheckOutFrom, request.CheckOutTo, "Check-out");
            ValidatePaging(request.PageNumber, request.PageSize);
            ValidateOrderBy(request.OrderBy);
        }

        public static void ValidateGetAvailableRoomsRequest(Guid hotelId, GetAvailableReservationRoomsRequestDTO request)
        {
            ValidateHotelId(hotelId);

            if (request is null)
                throw new BadRequestException("Request query is required.");

            ValidateReservationDates(request.CheckInDate, request.CheckOutDate);
        }

        private static void ValidateReservationDates(DateTime checkInDate, DateTime checkOutDate)
        {
            var normalizedCheckIn = checkInDate.Date;
            var normalizedCheckOut = checkOutDate.Date;
            var today = DateTime.UtcNow.Date;

            if (normalizedCheckIn < today)
                throw new BadRequestException("Check-in date must be today or later.");

            if (normalizedCheckOut <= normalizedCheckIn)
                throw new BadRequestException("Check-out date must be greater than check-in date.");
        }

        private static void ValidateRoomIds(List<Guid> roomIds)
        {
            if (roomIds is null || roomIds.Count == 0)
                throw new BadRequestException("At least one room must be selected.");

            if (roomIds.Any(roomId => roomId == Guid.Empty))
                throw new BadRequestException("Room ID must be provided.");

            if (roomIds.Distinct().Count() != roomIds.Count)
                throw new BadRequestException("Duplicate room IDs are not allowed.");
        }

        private static void ValidateGuid(Guid value, string fieldName)
        {
            if (value == Guid.Empty)
                throw new BadRequestException($"{fieldName} must be provided.");
        }

        private static void ValidateDateFilterRange(DateTime? from, DateTime? to, string fieldName)
        {
            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                throw new BadRequestException($"{fieldName} 'from' date must be less than or equal to 'to' date.");
        }

        private static void ValidatePaging(int? pageNumber, int? pageSize)
        {
            if (pageNumber.HasValue ^ pageSize.HasValue)
                throw new BadRequestException("Page number and page size must be provided together.");

            if (pageNumber.HasValue && pageNumber.Value <= 0)
                throw new BadRequestException("Page number must be greater than 0.");

            if (pageSize.HasValue && pageSize.Value <= 0)
                throw new BadRequestException("Page size must be greater than 0.");
        }

        private static void ValidateOrderBy(string orderBy)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
                return;

            var normalizedOrderBy = orderBy.Trim();

            if (!AllowedOrderByFields.Contains(normalizedOrderBy))
                throw new BadRequestException(
                    $"Invalid order by field. Allowed fields are: {string.Join(", ", AllowedOrderByFields)}.");
        }
    }
}
