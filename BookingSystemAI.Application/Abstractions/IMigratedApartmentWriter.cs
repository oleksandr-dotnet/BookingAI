using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Abstractions;

public interface IMigratedApartmentWriter
{
    Task AddAsync(Apartment apartment, CancellationToken cancellationToken = default);
}
