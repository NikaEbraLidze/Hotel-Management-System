using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Domain.Entities;

namespace hms.Application.Validation
{
    public static class RoomsValidation
    {
        private const decimal MinPrice = 0.01m;
        private const decimal MaxPrice = 10000m;
        private const int NameMaxLength = 10;
        private const int PriceScale = 2;

        private static readonly HashSet<string> AllowedOrderByFields = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Room.Id),
            nameof(Room.Name),
            nameof(Room.Price),
        };

        public static void ValidateHotelId(Guid hotelId)
        {
            ValidateGuid(hotelId, "Hotel ID");
        }

        public static void ValidateRoomId(Guid roomId)
        {
            ValidateGuid(roomId, "Room ID");
        }

        public static void ValidateRoomRoute(Guid hotelId, Guid roomId)
        {
            ValidateHotelId(hotelId);
            ValidateRoomId(roomId);
        }

        public static void ValidateGetRoomsRequest(Guid hotelId, GetRoomsRequestDTO request)
        {
            ValidateHotelId(hotelId);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            if (request.Price.HasValue)
                ValidatePrice(request.Price.Value, "Price filter");

            if (request.MinPrice.HasValue)
                ValidatePrice(request.MinPrice.Value, "Min price filter");

            if (request.MaxPrice.HasValue)
                ValidatePrice(request.MaxPrice.Value, "Max price filter");

            if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice.Value > request.MaxPrice.Value)
                throw new BadRequestException("Min price must be less than or equal to max price.");

            ValidatePaging(request.PageNumber, request.PageSize);
            ValidateMaxLength(request.Name, "Name filter", NameMaxLength);
            ValidateOrderBy(request.OrderBy);
        }

        public static void ValidateGetAvailableRoomsRequest(Guid hotelId, GetAvailableRoomsRequestDTO request)
        {
            ValidateHotelId(hotelId);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            if (request.CheckIn.HasValue ^ request.CheckOut.HasValue)
                throw new BadRequestException("Check-in and check-out must be provided together.");

            if (request.CheckIn.HasValue && request.CheckOut.HasValue && request.CheckOut.Value <= request.CheckIn.Value)
                throw new BadRequestException("Check-out must be greater than check-in.");
        }

        public static void ValidateCreateRoomRequest(Guid hotelId, CreateRoomRequestDTO request)
        {
            ValidateHotelId(hotelId);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            ValidateRequiredName(request.Name);
            ValidatePrice(request.Price, "Price");
        }

        public static void ValidateUpdateRoomRequest(Guid hotelId, Guid roomId, UpdateRoomRequestDTO request)
        {
            ValidateRoomRoute(hotelId, roomId);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            if (request.Name is null && !request.Price.HasValue)
                throw new BadRequestException("At least one field must be provided for update.");

            if (request.Name is not null)
                ValidateRequiredName(request.Name);

            if (request.Price.HasValue)
                ValidatePrice(request.Price.Value, "Price");
        }

        private static void ValidateGuid(Guid value, string fieldName)
        {
            if (value == Guid.Empty)
                throw new BadRequestException($"{fieldName} must be provided.");
        }

        private static void ValidateRequiredName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException("Name is required.");

            ValidateMaxLength(value, "Name", NameMaxLength);
        }

        private static void ValidatePrice(decimal value, string fieldName)
        {
            if (value < MinPrice || value > MaxPrice)
                throw new BadRequestException($"{fieldName} must be between {MinPrice} and {MaxPrice}.");

            if (decimal.Round(value, PriceScale) != value)
                throw new BadRequestException($"{fieldName} must have at most {PriceScale} decimal places.");
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

        private static void ValidateMaxLength(string value, string fieldName, int maxLength)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
                throw new BadRequestException($"{fieldName} must not exceed {maxLength} characters.");
        }
    }
}
