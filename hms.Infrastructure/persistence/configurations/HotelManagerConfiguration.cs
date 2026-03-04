using hms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace hms.Infrastructure.Persistence.Configurations
{
    public class HotelManagerConfiguration : IEntityTypeConfiguration<HotelManager>
    {
        public void Configure(EntityTypeBuilder<HotelManager> builder)
        {
            builder.ToTable("HotelManagers");

            builder.HasKey(x => new { x.HotelId, x.ManagerUserId });

            builder.HasOne(x => x.Hotel)
                .WithMany(h => h.HotelManagers)
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ManagerUser)
                .WithMany(u => u.ManagedHotels)
                .HasForeignKey(x => x.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ManagerUserId);
        }
    }
}
