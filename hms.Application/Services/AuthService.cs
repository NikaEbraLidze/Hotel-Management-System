using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Domain.Identity;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;

namespace hms.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Invalid credentials.";

        private readonly IUserRepository _users;
        private readonly IJWTTokenGenerator _jwt;
        private readonly IMapper _mapper;

        public AuthService(
            IUserRepository users,
            IJWTTokenGenerator jwt,
            IMapper mapper)
        {
            _users = users;
            _jwt = jwt;
            _mapper = mapper;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            ArgumentNullException.ThrowIfNull(loginRequestDTO);

            if (string.IsNullOrWhiteSpace(loginRequestDTO.Email))
                throw new ArgumentException("Email is required.", nameof(loginRequestDTO));

            if (string.IsNullOrWhiteSpace(loginRequestDTO.Password))
                throw new ArgumentException("Password is required.", nameof(loginRequestDTO));

            var email = loginRequestDTO.Email.Trim();
            var user = await _users.GetUserByEmailAsync(email)
                ?? throw new UnauthorizedAccessException(InvalidCredentialsMessage);

            if (!await _users.CheckPasswordAsync(user, loginRequestDTO.Password))
                throw new UnauthorizedAccessException(InvalidCredentialsMessage);

            var userRoles = await _users.GetRolesAsync(user);

            var token = _jwt.GenerateToken(user, userRoles);

            return new LoginResponseDTO
            {
                Token = token
            };

        }

        public Task<string> RegisterAdminAsync(RegistrationRequestDTO registrationRequestDTO)
            => RegisterAsync(registrationRequestDTO, AppRole.Admin);

        public Task<string> RegisterGuestAsync(RegistrationRequestDTO registrationRequestDTO)
            => RegisterAsync(registrationRequestDTO, AppRole.Guest);

        public Task<string> RegisterManagerAsync(RegistrationRequestDTO registrationRequestDTO)
            => RegisterAsync(registrationRequestDTO, AppRole.Manager);

        private async Task<string> RegisterAsync(RegistrationRequestDTO registrationRequestDTO, AppRole role)
        {
            ValidateRegistrationRequest(registrationRequestDTO);

            var user = _mapper.Map<ApplicationUser>(registrationRequestDTO);

            var result = await _users.CreateUserAsync(user, registrationRequestDTO.Password);
            ThrowIfIdentityOperationFailed(result, "User registration failed");

            var addToRoleResult = await _users.AddToRoleAsync(user, role.ToRoleName());
            ThrowIfIdentityOperationFailed(addToRoleResult, $"Failed to assign {role.ToRoleName()} role");

            return user.Id.ToString();
        }

        private static void ValidateRegistrationRequest(RegistrationRequestDTO registrationRequestDTO)
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

        private static void ThrowIfIdentityOperationFailed(IdentityResult result, string errorPrefix)
        {
            if (result.Succeeded)
                return;

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"{errorPrefix}: {errors}");
        }
    }
}
