using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Domain.Entities;
using BookingSystemAI.Infrastructure.Data;
using BookingSystemAI.Infrastructure.Data.Entities;
using BookingSystemAI.Infrastructure.Sql;

namespace BookingSystemAI.Infrastructure.CompanyImport;

public sealed class MigratedApartmentWriter(ApplicationDbContext dbContext) : IMigratedApartmentWriter
{
    public Task AddAsync(Apartment apartment, CancellationToken cancellationToken = default)
    {
        dbContext.Apartments.Add(new ApartmentRecord
        {
            Id = apartment.Id,
            HostId = apartment.HostId,
            Name = apartment.Name,
            Description = apartment.Description,
            PricePerNight = apartment.PricePerNight,
            GuestCount = apartment.GuestCount,
            Amenities = EntityMapping.MapAmenityNames(apartment.Amenities),
            MetadataJson = apartment.MetadataJson,
            SourceCompanyId = apartment.SourceCompanyId,
            ExternalId = apartment.ExternalId
        });
        return Task.CompletedTask;
    }
}
