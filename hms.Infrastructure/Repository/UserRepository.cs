using hms.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hms.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
        }

        public Task AddToRoleAsync(ApplicationUser user, string role) => _userManager.AddToRoleAsync(user, role);

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password) => _userManager.CheckPasswordAsync(user, password);

        public Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token) => _userManager.ConfirmEmailAsync(user, token);

        public Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password) => _userManager.CreateAsync(user, password);

        public async Task<IdentityResult> DeleteUserAsync(Guid userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user is null)
                return IdentityResult.Failed();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

            return result;
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user) => _userManager.GenerateEmailConfirmationTokenAsync(user);

        public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) => _userManager.GeneratePasswordResetTokenAsync(user);

        public Task<List<ApplicationUser>> GetAllUsersAsync() => _userManager.Users.ToListAsync();

        public Task<IList<string>> GetRolesAsync(ApplicationUser user) => _userManager.GetRolesAsync(user);

        public Task<ApplicationUser> GetUserByEmailAsync(string email) => _userManager.FindByEmailAsync(email);

        public async Task<ApplicationUser> GetUserByIdAsync(Guid userId) => await _userManager.FindByIdAsync(userId.ToString());

        public Task RemoveFromRoleAsync(ApplicationUser user, string role) => _userManager.RemoveFromRoleAsync(user, role);

        public Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword) =>
            _userManager.ResetPasswordAsync(user, token, newPassword);

        public Task<IdentityResult> UpdateUserAsync(ApplicationUser user) => _userManager.UpdateAsync(user);

        public async Task<bool> UserExistsByEmailAsync(string email)
            => await _userManager.FindByEmailAsync(email) != null;
    }
}