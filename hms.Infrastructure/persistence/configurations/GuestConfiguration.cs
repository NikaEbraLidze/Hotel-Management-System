namespace hms.Infrastructure.persistence.configurations
{
    public class GuestConfiguration : IEntityTypeConfiguration<Guest>
    {
        public void Configure(EntityTypeBuilder<Guest> builder)
        {
            builder.ToTable("Guests");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.PersonalNumber).IsRequired().HasMaxLength(11);
            builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(15);

            // Unique constraints
            builder.HasIndex(x => x.PersonalNumber).IsUnique();
            builder.HasIndex(x => x.PhoneNumber).IsUnique();
        }
    }
}