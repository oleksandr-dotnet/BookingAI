using BookingSystemAI.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystemAI.Infrastructure.Data.Configurations;

public sealed class ApartmentRecordConfiguration : IEntityTypeConfiguration<ApartmentRecord>
{
    public void Configure(EntityTypeBuilder<ApartmentRecord> builder)
    {
        builder.ToTable("Apartments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.HostId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.PricePerNight).HasPrecision(18, 2).IsRequired();
        builder.Property(a => a.GuestCount).IsRequired();
        builder.Property(a => a.Amenities).HasColumnType("text[]").IsRequired();
        builder.Property(a => a.MetadataJson).HasColumnType("jsonb").IsRequired().HasDefaultValueSql("'{}'::jsonb");
        builder.Property(a => a.ExternalId).HasMaxLength(128);
        builder.HasIndex(a => a.HostId);
        builder.HasIndex(a => new { a.SourceCompanyId, a.ExternalId })
            .IsUnique()
            .HasFilter("\"SourceCompanyId\" IS NOT NULL AND \"ExternalId\" IS NOT NULL");
    }
}
