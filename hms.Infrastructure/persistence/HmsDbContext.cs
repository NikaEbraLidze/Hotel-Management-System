using hms.Domain.Entities;
using hms.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace hms.Infrastructure.Persistence
{
    public class HmsDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<HotelManager> HotelManagers => Set<HotelManager>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<ReservationRoom> ReservationRooms => Set<ReservationRoom>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically apply all IEntityTypeConfiguration<> from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HmsDbContext).Assembly);
        }
    }
}
