using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Abstractions;

public interface IApartmentRepository
{
    Task<IReadOnlyList<Apartment>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Apartment>> ListByHostIdAsync(string hostId, CancellationToken cancellationToken = default);
    Task<Apartment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Apartment?> GetByExternalAsync(Guid sourceCompanyId, string externalId,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Apartment apartment, CancellationToken cancellationToken = default);
    Task<ApartmentUpdateOutcome> TryUpdateAsync(Apartment apartment, int expectedVersion,
        CancellationToken cancellationToken = default);
}

public enum ApartmentUpdateOutcome
{
    Success,
    NotFound,
    Conflict
}
