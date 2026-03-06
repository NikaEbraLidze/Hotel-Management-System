using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Domain.Identity;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using hms.Application.Validation;
using hms.Application.Models.Exceptions;

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
            AuthValidation.ValidateLoginRequest(loginRequestDTO);

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
            AuthValidation.ValidateRegistrationRequest(registrationRequestDTO);

            var user = _mapper.Map<ApplicationUser>(registrationRequestDTO);

            var result = await _users.CreateUserAsync(user, registrationRequestDTO.Password);
            ThrowIfIdentityOperationFailed(result, "User registration failed");

            var addToRoleResult = await _users.AddToRoleAsync(user, role.ToRoleName());
            ThrowIfIdentityOperationFailed(addToRoleResult, $"Failed to assign {role.ToRoleName()} role");

            return user.Id.ToString();
        }

        private static void ThrowIfIdentityOperationFailed(IdentityResult result, string errorPrefix)
        {
            if (result.Succeeded)
                return;

            var errors = result.Errors
                .Select(e => $"{errorPrefix}: {e.Description}")
                .ToList();

            throw new IdentityOperationException(errors);
        }
    }
}
