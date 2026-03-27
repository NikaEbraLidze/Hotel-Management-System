using hms.Application.Models.DTO;

namespace hms.Application.Contracts.Service
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task<RegistrationResponseDTO> RegisterGuestAsync(RegistrationRequestDTO registrationRequestDTO);
        Task<RegistrationResponseDTO> RegisterManagerAsync(RegistrationRequestDTO registrationRequestDTO);
        Task<RegistrationResponseDTO> RegisterAdminAsync(RegistrationRequestDTO registrationRequestDTO);
        Task ConfirmEmailAsync(string userId, string token);
    }
}
