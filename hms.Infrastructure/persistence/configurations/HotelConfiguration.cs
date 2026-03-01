using hms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace hms.Infrastructure.Persistence.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.ToTable("Hotels");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Rating).IsRequired();
            builder.Property(x => x.Address).IsRequired().HasMaxLength(200);
            builder.Property(x => x.City).IsRequired().HasMaxLength(200);

            // Relationships
            builder.HasMany(h => h.Rooms)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(h => h.Managers)
                .WithOne(m => m.Hotel)
                .HasForeignKey(m => m.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            // rating check constraint (SQL Server)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Hotels_Rating", "[Rating] >= 1 AND [Rating] <= 5");
            });
        }
    }
}