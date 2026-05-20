namespace BookingSystemAI.Domain.Entities;

public sealed class Booking
{
    public required Guid Id { get; init; }
    public required Guid ApartmentId { get; init; }
    public required string UserId { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset End { get; init; }
}
