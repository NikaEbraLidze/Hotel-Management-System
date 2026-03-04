
using hms.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace hms.Application.Contracts.Repository
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(Guid userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<IdentityResult> CreateUserAsync(ApplicationUser user);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(Guid userId);

        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task AddToRoleAsync(ApplicationUser user, string role);
    }
}