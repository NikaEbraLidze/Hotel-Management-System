using hms.Application.Contracts.Repository;
using hms.Domain.Entities;
using hms.Domain.Identity;
using hms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hms.Infrastructure.Repository
{
    public class GuestsRepository : IdentityRepositoryBase<ApplicationUser>, IGuestsRepository
    {
        public GuestsRepository(UserManager<ApplicationUser> userManager, HmsDbContext context)
            : base(userManager, context)
        {
        }

        protected override AppRole Role => AppRole.Guest;

        public Task<bool> HasReservationsAsync(Guid guestId)
        {
            return _context.Set<Reservation>().AnyAsync(reservation => reservation.GuestId == guestId);
        }
    }
}
