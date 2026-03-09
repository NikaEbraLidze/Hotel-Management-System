using hms.Domain.Identity;

namespace hms.Application.Contracts.Repository
{
    public interface IGuestsRepository : IIdentityRepositoryBase<ApplicationUser>
    {
        Task<bool> HasReservationsAsync(Guid guestId);
    }
}
