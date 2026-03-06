using hms.Application.Models.DTO;

namespace hms.Application.Validation
{
    public class AuthValidation
    {
        public static void ValidateRegistrationRequest(RegistrationRequestDTO registrationRequestDTO)
        {
            ArgumentNullException.ThrowIfNull(registrationRequestDTO);

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.FirstName))
                throw new ArgumentException("First name is required.", nameof(registrationRequestDTO));

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.LastName))
                throw new ArgumentException("Last name is required.", nameof(registrationRequestDTO));

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.Email))
                throw new ArgumentException("Email is required.", nameof(registrationRequestDTO));

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.Password))
                throw new ArgumentException("Password is required.", nameof(registrationRequestDTO));

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.PersonalNumber))
                throw new ArgumentException("Personal number is required.", nameof(registrationRequestDTO));

            if (string.IsNullOrWhiteSpace(registrationRequestDTO.PhoneNumber))
                throw new ArgumentException("Phone number is required.", nameof(registrationRequestDTO));
        }

        public static void ValidateLoginRequest(LoginRequestDTO loginRequestDTO)
        {
            ArgumentNullException.ThrowIfNull(loginRequestDTO);

            if (string.IsNullOrWhiteSpace(loginRequestDTO.Email))
                throw new ArgumentException("Email is required.", nameof(loginRequestDTO));

            if (string.IsNullOrWhiteSpace(loginRequestDTO.Password))
                throw new ArgumentException("Password is required.", nameof(loginRequestDTO));
        }
    }
}