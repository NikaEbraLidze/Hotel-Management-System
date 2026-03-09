using hms.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace hms.Application.Contracts.Repository
{
    public interface IIdentityRepositoryBase<TUser> where TUser : ApplicationUser
    {
        Task<TUser> GetByIdAsync(Guid userId);
        Task<List<TUser>> GetAllAsync();
        Task<bool> UserExistsByEmailAsync(string email);
        Task<IdentityResult> CreateAsync(TUser user, string password);
        Task<IdentityResult> UpdateAsync(TUser user);
        Task<IdentityResult> DeleteAsync(TUser user);
    }
}
