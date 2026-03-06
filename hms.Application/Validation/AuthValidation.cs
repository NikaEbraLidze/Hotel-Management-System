using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;

namespace hms.Application.Validation
{
    public class AuthValidation
    {
        public static void ValidateRegistrationRequest(RegistrationRequestDTO registrationRequestDTO)
        {
            if (registrationRequestDTO is null)
                throw new BadRequestException("Request body is required.");

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.FirstName))
                throw new BadRequestException("First name is required.");

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.LastName))
                throw new BadRequestException("Last name is required.");

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.Email))
                throw new BadRequestException("Email is required.");

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.Password))
                throw new BadRequestException("Password is required.");

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.PersonalNumber))
                throw new BadRequestException("Personal number is required.");

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.PhoneNumber))
                throw new BadRequestException("Phone number is required.");
        }

        public static void ValidateLoginRequest(LoginRequestDTO loginRequestDTO)
        {
            if (loginRequestDTO is null)
                throw new BadRequestException("Request body is required.");

            if (string.IsNullOrWhiteSpace(loginRequestDTO.Email))
                throw new BadRequestException("Email is required.");

            if (string.IsNullOrWhiteSpace(loginRequestDTO.Password))
                throw new BadRequestException("Password is required.");
        }
    }
}
