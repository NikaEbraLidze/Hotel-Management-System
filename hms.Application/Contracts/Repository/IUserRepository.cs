using hms.Domain.Identity;
using Microsoft.AspNetCore.Identity;

public interface IUserRepository
{
    Task<ApplicationUser> GetUserByIdAsync(Guid userId);
    Task<ApplicationUser> GetUserByEmailAsync(string email);
    Task<List<ApplicationUser>> GetAllUsersAsync();

    Task<bool> UserExistsByEmailAsync(string email);

    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    Task<IdentityResult> DeleteUserAsync(Guid userId);

    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task AddToRoleAsync(ApplicationUser user, string role);
    Task RemoveFromRoleAsync(ApplicationUser user, string role);

    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);

    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
}