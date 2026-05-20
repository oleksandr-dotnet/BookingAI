using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.CompanyImport;

public sealed class ExternalEntityLookup(ApplicationDbContext dbContext) : IExternalEntityLookup
{
    public Task<bool> UserExistsAsync(Guid sourceCompanyId, string externalId,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(
            u => u.SourceCompanyId == sourceCompanyId && u.ExternalId == externalId,
            cancellationToken);

    public Task<bool> ApartmentExistsAsync(Guid sourceCompanyId, string externalId,
        CancellationToken cancellationToken = default) =>
        dbContext.Apartments.AnyAsync(
            a => a.SourceCompanyId == sourceCompanyId && a.ExternalId == externalId,
            cancellationToken);

    public async Task<string?> FindUserIdAsync(Guid sourceCompanyId, string externalId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking()
            .Where(u => u.SourceCompanyId == sourceCompanyId && u.ExternalId == externalId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return user;
    }

    public async Task<Guid?> FindApartmentIdAsync(Guid sourceCompanyId, string externalId,
        CancellationToken cancellationToken = default)
    {
        var apartment = await dbContext.Apartments.AsNoTracking()
            .Where(a => a.SourceCompanyId == sourceCompanyId && a.ExternalId == externalId)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return apartment;
    }
}
