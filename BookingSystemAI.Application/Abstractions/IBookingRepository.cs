using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Abstractions;

public interface IBookingRepository
{
    Task<bool> HasOverlapAsync(Guid apartmentId, DateTimeOffset start, DateTimeOffset end,
        CancellationToken cancellationToken = default);

    Task<bool> TryAddAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> ListByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
