using BookingSystemAI.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystemAI.Infrastructure.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.ExternalId).HasMaxLength(128);
        builder.HasIndex(u => new { u.SourceCompanyId, u.ExternalId })
            .IsUnique()
            .HasFilter("\"SourceCompanyId\" IS NOT NULL AND \"ExternalId\" IS NOT NULL");
    }
}
