namespace hms.Infrastructure.persistence.configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Number).IsRequired().HasMaxLength(10);
            builder.Property(r => r.Price).IsRequired().HasColumnType("decimal(18,2)");

            builder.Property(x => x.HotelId)
                .IsRequired();

            // Configure the relationship with hotel
            builder.HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.HotelId);
        }
    }
}