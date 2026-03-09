using hms.Application.Contracts.Repository;
using hms.Domain.Identity;
using hms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace hms.Infrastructure.Repository
{
    public abstract class IdentityRepositoryBase<TUser> : IIdentityRepositoryBase<TUser>
        where TUser : ApplicationUser
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly HmsDbContext _context;

        protected IdentityRepositoryBase(
            UserManager<ApplicationUser> userManager,
            HmsDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        protected abstract AppRole Role { get; }

        public async Task<TUser> GetByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is not TUser typedUser)
                return null;

            return await _userManager.IsInRoleAsync(typedUser, Role.ToRoleName())
                ? typedUser
                : null;
        }

        public async Task<List<TUser>> GetAllAsync()
        {
            var users = await _userManager.GetUsersInRoleAsync(Role.ToRoleName());
            return users.OfType<TUser>().ToList();
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        public async Task<IdentityResult> CreateAsync(TUser user, string password)
        {
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
                return createResult;

            var addToRoleResult = await _userManager.AddToRoleAsync(user, Role.ToRoleName());

            if (addToRoleResult.Succeeded)
                return addToRoleResult;

            await _userManager.DeleteAsync(user);
            return addToRoleResult;
        }

        public Task<IdentityResult> UpdateAsync(TUser user)
        {
            return _userManager.UpdateAsync(user);
        }

        public Task<IdentityResult> DeleteAsync(TUser user)
        {
            return _userManager.DeleteAsync(user);
        }
    }
}
