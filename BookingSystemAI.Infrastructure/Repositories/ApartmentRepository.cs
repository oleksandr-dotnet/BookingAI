using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Domain.Entities;
using BookingSystemAI.Infrastructure.Data;
using BookingSystemAI.Infrastructure.Data.Entities;
using BookingSystemAI.Infrastructure.Sql;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.Repositories;

public sealed class ApartmentRepository(ApplicationDbContext dbContext) : IApartmentRepository
{
    public async Task<IReadOnlyList<Apartment>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Apartments.AsNoTracking().OrderBy(a => a.Name).ToListAsync(cancellationToken);
        return records.Select(EntityMapping.MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Apartment>> ListByHostIdAsync(string hostId,
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Apartments
            .AsNoTracking()
            .Where(a => a.HostId == hostId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
        return records.Select(EntityMapping.MapToDomain).ToList();
    }

    public async Task<Apartment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Apartments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        return record is null ? null : EntityMapping.MapToDomain(record);
    }

    public async Task<Apartment?> GetByExternalAsync(Guid sourceCompanyId, string externalId,
        CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Apartments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.SourceCompanyId == sourceCompanyId && a.ExternalId == externalId,
                cancellationToken);
        return record is null ? null : EntityMapping.MapToDomain(record);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Apartments.AnyAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Apartment apartment, CancellationToken cancellationToken = default)
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
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
