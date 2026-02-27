namespace hms.Infrastructure.persistence.configurations
{
    public class ReservationRoomConfiguration : IEntityTypeConfiguration<ReservationRoom>
    {
        public void Configure(EntityTypeBuilder<ReservationRoom> builder)
        {
            builder.ToTable("ReservationRooms");

            builder.HasKey(rr => new { rr.ReservationId, rr.RoomId });

            builder.HasOne(rr => rr.Reservation)
                .WithMany(r => r.ReservationRooms)
                .HasForeignKey(rr => rr.ReservationId);

            builder.HasOne(rr => rr.Room)
                .WithMany(r => r.ReservationRooms)
                .HasForeignKey(rr => rr.RoomId);
        }
    }
}