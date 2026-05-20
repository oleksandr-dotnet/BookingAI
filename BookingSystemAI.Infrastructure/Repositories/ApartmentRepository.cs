using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Domain.Entities;
using BookingSystemAI.Infrastructure.Data;
using BookingSystemAI.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.Repositories;

public sealed class ApartmentRepository(ApplicationDbContext dbContext) : IApartmentRepository
{
    public async Task<IReadOnlyList<Apartment>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Apartments.AsNoTracking().OrderBy(a => a.Name).ToListAsync(cancellationToken);
        return records.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Apartment>> ListByHostIdAsync(string hostId,
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Apartments
            .AsNoTracking()
            .Where(a => a.HostId == hostId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
        return records.Select(MapToDomain).ToList();
    }

    public async Task<Apartment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Apartments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        return record is null ? null : MapToDomain(record);
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
            Description = apartment.Description
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Apartment MapToDomain(ApartmentRecord record) =>
        new()
        {
            Id = record.Id,
            HostId = record.HostId,
            Name = record.Name,
            Description = record.Description,
            SourceCompanyId = record.SourceCompanyId,
            ExternalId = record.ExternalId
        };
}
