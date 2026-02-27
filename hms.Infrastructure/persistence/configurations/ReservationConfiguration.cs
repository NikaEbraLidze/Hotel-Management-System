namespace hms.Infrastructure.persistence.configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("Reservations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CheckInDate)
                .IsRequired();

            builder.Property(x => x.CheckOutDate)
                .IsRequired();

            builder.Property(x => x.GuestId)
                .IsRequired();

            builder.HasOne(x => x.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(x => x.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.GuestId);

            // avoid invalid ranges at DB level (still validate in Application too)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Reservations_DateRange", "[CheckOutDate] > [CheckInDate]");
            });
        }
    }
}