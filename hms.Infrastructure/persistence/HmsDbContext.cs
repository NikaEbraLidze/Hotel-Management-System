using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hms.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using hms.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace hms.Infrastructure.Persistence
{
    public class HmsDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Manager> Managers => Set<Manager>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<ReservationRoom> ReservationRooms => Set<ReservationRoom>();
        public DbSet<Guest> Guests => Set<Guest>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically apply all IEntityTypeConfiguration<> from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HmsDbContext).Assembly);
        }
    }
}