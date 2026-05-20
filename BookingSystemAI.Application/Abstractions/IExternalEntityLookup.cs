namespace BookingSystemAI.Application.Abstractions;

public interface IExternalEntityLookup
{
    Task<bool> UserExistsAsync(Guid sourceCompanyId, string externalId, CancellationToken cancellationToken = default);
    Task<bool> ApartmentExistsAsync(Guid sourceCompanyId, string externalId, CancellationToken cancellationToken = default);
    Task<string?> FindUserIdAsync(Guid sourceCompanyId, string externalId, CancellationToken cancellationToken = default);
    Task<Guid?> FindApartmentIdAsync(Guid sourceCompanyId, string externalId, CancellationToken cancellationToken = default);
}
