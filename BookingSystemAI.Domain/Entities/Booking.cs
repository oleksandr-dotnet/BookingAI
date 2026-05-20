namespace BookingSystemAI.Domain.Entities;

public sealed class Booking
{
    public required Guid Id { get; init; }
    public required Guid ApartmentId { get; init; }
    public required string UserId { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset End { get; init; }
    public required decimal PricePerNight { get; init; }
    public required int GuestCount { get; init; }
    public IReadOnlyList<Amenity> Amenities { get; init; } = [];
}
