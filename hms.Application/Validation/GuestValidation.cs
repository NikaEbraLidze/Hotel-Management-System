using System.Net.Mail;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;

namespace hms.Application.Validation
{
    public static class GuestValidation
    {
        private const int FirstNameMaxLength = 50;
        private const int LastNameMaxLength = 50;
        private const int EmailMaxLength = 256;
        private const int PersonalNumberLength = 11;
        private const int PhoneNumberMaxLength = 15;

        public static void ValidateGuestId(Guid guestId)
        {
            if (guestId == Guid.Empty)
                throw new BadRequestException("Guest ID must be provided.");
        }

        public static void ValidateUpdateGuestRequest(Guid guestId, UpdateGuestRequestDTO request)
        {
            ValidateGuestId(guestId);

            if (request is null)
                throw new BadRequestException("Request body is required.");

            if (request.FirstName is null &&
                request.LastName is null &&
                request.Email is null &&
                request.PersonalNumber is null &&
                request.PhoneNumber is null)
            {
                throw new BadRequestException("At least one field must be provided for update.");
            }

            if (request.FirstName is not null)
                ValidateRequiredText(request.FirstName, "First name", FirstNameMaxLength);

            if (request.LastName is not null)
                ValidateRequiredText(request.LastName, "Last name", LastNameMaxLength);

            if (request.Email is not null)
                ValidateRequiredEmail(request.Email);

            if (request.PersonalNumber is not null)
                ValidateRequiredPersonalNumber(request.PersonalNumber);

            if (request.PhoneNumber is not null)
                ValidateRequiredPhoneNumber(request.PhoneNumber);
        }

        private static void ValidateRequiredText(string value, string fieldName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException($"{fieldName} is required.");

            ValidateMaxLength(value, fieldName, maxLength);
        }

        private static void ValidateRequiredEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException("Email is required.");

            ValidateMaxLength(value, "Email", EmailMaxLength);

            try
            {
                _ = new MailAddress(value.Trim());
            }
            catch (FormatException)
            {
                throw new BadRequestException("Email format is invalid.");
            }
        }

        private static void ValidateRequiredPersonalNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException("Personal number is required.");

            var normalizedValue = value.Trim();

            if (normalizedValue.Length != PersonalNumberLength || !normalizedValue.All(char.IsDigit))
                throw new BadRequestException($"Personal number must be exactly {PersonalNumberLength} digits.");
        }

        private static void ValidateRequiredPhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException("Phone number is required.");

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > PhoneNumberMaxLength)
                throw new BadRequestException($"Phone number must not exceed {PhoneNumberMaxLength} characters.");

            var plusCount = normalizedValue.Count(c => c == '+');

            if (plusCount > 1 || (plusCount == 1 && normalizedValue[0] != '+'))
                throw new BadRequestException("Phone number format is invalid.");

            if (!normalizedValue.All(c => char.IsDigit(c) || c == '+'))
                throw new BadRequestException("Phone number format is invalid.");
        }

        private static void ValidateMaxLength(string value, string fieldName, int maxLength)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
                throw new BadRequestException($"{fieldName} must not exceed {maxLength} characters.");
        }
    }
}
