using BookingSystemAI.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystemAI.Infrastructure.Data.Configurations;

public sealed class BookingRecordConfiguration : IEntityTypeConfiguration<BookingRecord>
{
    public void Configure(EntityTypeBuilder<BookingRecord> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.UserId).HasMaxLength(450).IsRequired();
        builder.Property(b => b.PricePerNight).HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.GuestCount).IsRequired();
        builder.Property(b => b.Amenities).HasColumnType("text[]").IsRequired();
        builder.HasIndex(b => new { b.ApartmentId, b.Start, b.End });
        builder.HasIndex(b => b.UserId);
        builder.HasOne(b => b.Apartment)
            .WithMany(a => a.Bookings)
            .HasForeignKey(b => b.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
