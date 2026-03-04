using hms.Application.Models.DTO;

namespace hms.Application.Contracts.Service
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task<string> RegisterGuestAsync(RegistrationRequestDTO registrationRequestDTO);
        Task<string> RegisterManagerAsync(RegistrationRequestDTO registrationRequestDTO);
        Task<string> RegisterAdminAsync(RegistrationRequestDTO registrationRequestDTO);
    }
}