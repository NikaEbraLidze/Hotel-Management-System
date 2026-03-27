using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Domain.Identity;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using hms.Application.Validation;
using hms.Application.Models.Exceptions;
using hms.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace hms.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Invalid credentials.";
        private const string EmailNotConfirmedMessage = "Email address is not confirmed.";

        private readonly IUserRepository _users;
        private readonly IJWTTokenGenerator _jwt;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly AppUrlConfiguration _appUrlConfiguration;

        public AuthService(
            IUserRepository users,
            IJWTTokenGenerator jwt,
            IMapper mapper,
            IEmailService emailService,
            IOptions<AppUrlConfiguration> appUrlConfiguration)
        {
            _users = users;
            _jwt = jwt;
            _mapper = mapper;
            _emailService = emailService;
            _appUrlConfiguration = appUrlConfiguration.Value;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            AuthValidation.ValidateLoginRequest(loginRequestDTO);

            var email = loginRequestDTO.Email.Trim();
            var user = await _users.GetUserByEmailAsync(email)
                ?? throw new UnauthorizedAccessException(InvalidCredentialsMessage);

            if (!await _users.CheckPasswordAsync(user, loginRequestDTO.Password))
                throw new UnauthorizedAccessException(InvalidCredentialsMessage);

            if (!user.EmailConfirmed)
                throw new UnauthorizedAccessException(EmailNotConfirmedMessage);

            var userRoles = await _users.GetRolesAsync(user);

            var token = _jwt.GenerateToken(user, userRoles);

            return new LoginResponseDTO
            {
                Token = token
            };

        }

        public Task<RegistrationResponseDTO> RegisterAdminAsync(RegistrationRequestDTO registrationRequestDTO)
            => RegisterAsync(registrationRequestDTO, AppRole.Admin);

        public Task<RegistrationResponseDTO> RegisterGuestAsync(RegistrationRequestDTO registrationRequestDTO)
            => RegisterAsync(registrationRequestDTO, AppRole.Guest);

        public Task<RegistrationResponseDTO> RegisterManagerAsync(RegistrationRequestDTO registrationRequestDTO)
            => RegisterAsync(registrationRequestDTO, AppRole.Manager);

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new BadRequestException("Invalid user id.");

            if (string.IsNullOrWhiteSpace(token))
                throw new BadRequestException("Confirmation token is required.");

            var user = await _users.GetUserByIdAsync(parsedUserId)
                ?? throw new NotFoundException("User not found.");

            var result = await _users.ConfirmEmailAsync(user, token);
            ThrowIfIdentityOperationFailed(result, "Email confirmation failed");
        }

        private async Task<RegistrationResponseDTO> RegisterAsync(RegistrationRequestDTO registrationRequestDTO, AppRole role)
        {
            AuthValidation.ValidateRegistrationRequest(registrationRequestDTO);

            var user = _mapper.Map<ApplicationUser>(registrationRequestDTO);

            var result = await _users.CreateUserAsync(user, registrationRequestDTO.Password);
            ThrowIfIdentityOperationFailed(result, "User registration failed");

            var addToRoleResult = await _users.AddToRoleAsync(user, role.ToRoleName());
            ThrowIfIdentityOperationFailed(addToRoleResult, $"Failed to assign {role.ToRoleName()} role");

            try
            {
                var confirmationToken = await _users.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = BuildConfirmationLink(user.Id, confirmationToken);
                var emailBody = BuildConfirmationEmailBody(user.FirstName, confirmationLink);

                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Confirm your email",
                    emailBody,
                    isBodyHtml: true);
            }
            catch (SmtpException ex)
            {
                await _users.DeleteUserAsync(user.Id);
                throw new BadRequestException($"Confirmation email could not be sent. {ex.Message}", ex);
            }
            catch (Exception)
            {
                await _users.DeleteUserAsync(user.Id);
                throw;
            }

            return new RegistrationResponseDTO
            {
                UserId = user.Id.ToString()
            };
        }

        private string BuildConfirmationLink(Guid userId, string token)
        {
            var baseUrl = _appUrlConfiguration.ApiBaseUrl.TrimEnd('/');
            var encodedUserId = Uri.EscapeDataString(userId.ToString());
            var encodedToken = Uri.EscapeDataString(token);

            return $"{baseUrl}/api/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";
        }

        private static string BuildConfirmationEmailBody(string firstName, string confirmationLink)
        {
            return $"""
                <p>Hello {firstName},</p>
                <p>Thanks for registering in HMS.</p>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href="{confirmationLink}">Confirm email</a></p>
                <p>If you did not create this account, you can ignore this email.</p>
                """;
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
