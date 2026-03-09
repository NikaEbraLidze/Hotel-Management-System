using hms.Application.Contracts.Repository;
using hms.Domain.Identity;
using hms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace hms.Infrastructure.Repository
{
    public class ManagersRepository : IdentityRepositoryBase<ApplicationUser>, IManagersRepository
    {
        public ManagersRepository(UserManager<ApplicationUser> userManager, HmsDbContext context)
            : base(userManager, context)
        {
        }

        protected override AppRole Role => AppRole.Manager;
    }
}
