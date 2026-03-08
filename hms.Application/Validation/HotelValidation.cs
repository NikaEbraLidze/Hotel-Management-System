using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Domain.Entities;

namespace hms.Application.Validation
{
    public class HotelValidation
    {
        private const byte MinRating = 1;
        private const byte MaxRating = 5;
        private const int NameMaxLength = 100;
        private const int AddressMaxLength = 200;
        private const int CityMaxLength = 200;
        private const int CountryMaxLength = 200;

        private static readonly HashSet<string> AllowedOrderByFields = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Hotel.Id),
            nameof(Hotel.Name),
            nameof(Hotel.Rating),
            nameof(Hotel.Address),
            nameof(Hotel.City),
            nameof(Hotel.Country)
        };

        public static void ValidateGuid(Guid request)
        {
            if (request == Guid.Empty)
                throw new BadRequestException("Hotel ID must be provided.");
        }

        public static void ValidateUpdateHotelRequest(Guid id, UpdateHotelRequestDTO request)
        {
            ValidateGuid(id);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            if (request.Rating.HasValue && (request.Rating.Value < MinRating || request.Rating.Value > MaxRating))
                throw new BadRequestException($"Rating must be between {MinRating} and {MaxRating}.");

            ValidateMaxLength(request.Name, "Name", NameMaxLength);
            ValidateMaxLength(request.Address, "Address", AddressMaxLength);
            ValidateMaxLength(request.City, "City", CityMaxLength);
            ValidateMaxLength(request.Country, "Country", CountryMaxLength);
        }

        public static void ValidateGetHotelsRequest(GetHotelsRequestDTO request)
        {
            if (request is null)
                throw new BadRequestException("Request body is required.");

            if (request.Rating.HasValue && (request.Rating.Value < MinRating || request.Rating.Value > MaxRating))
                throw new BadRequestException($"Rating must be between {MinRating} and {MaxRating}.");

            if (request.PageNumber.HasValue ^ request.PageSize.HasValue)
                throw new BadRequestException("Page number and page size must be provided together.");

            if (request.PageNumber.HasValue && request.PageNumber.Value <= 0)
                throw new BadRequestException("Page number must be greater than 0.");

            if (request.PageSize.HasValue && request.PageSize.Value <= 0)
                throw new BadRequestException("Page size must be greater than 0.");

            ValidateMaxLength(request.Name, "Name filter", NameMaxLength);
            ValidateMaxLength(request.City, "City filter", CityMaxLength);
            ValidateMaxLength(request.Country, "Country filter", CountryMaxLength);

            if (!string.IsNullOrWhiteSpace(request.OrderBy))
            {
                var orderBy = request.OrderBy.Trim();

                if (!AllowedOrderByFields.Contains(orderBy))
                    throw new BadRequestException(
                        $"Invalid order by field. Allowed fields are: {string.Join(", ", AllowedOrderByFields)}.");
            }
        }

        public static void ValidateRegisterHotelRequest(RegisterHotelRequestDTO request)
        {
            if (request is null)
                throw new BadRequestException("Request body is required.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Name is required.");

            if (string.IsNullOrWhiteSpace(request.Address))
                throw new BadRequestException("Address is required.");

            if (string.IsNullOrWhiteSpace(request.City))
                throw new BadRequestException("City is required.");

            if (string.IsNullOrWhiteSpace(request.Country))
                throw new BadRequestException("Country is required.");

            if (request.Rating < MinRating || request.Rating > MaxRating)
                throw new BadRequestException($"Rating must be between {MinRating} and {MaxRating}.");

            ValidateMaxLength(request.Name, "Name", NameMaxLength);
            ValidateMaxLength(request.Address, "Address", AddressMaxLength);
            ValidateMaxLength(request.City, "City", CityMaxLength);
            ValidateMaxLength(request.Country, "Country", CountryMaxLength);
        }

        private static void ValidateMaxLength(string value, string fieldName, int maxLength)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
                throw new BadRequestException($"{fieldName} must not exceed {maxLength} characters.");
        }
    }
}
