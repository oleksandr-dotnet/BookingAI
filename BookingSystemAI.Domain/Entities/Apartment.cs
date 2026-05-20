namespace BookingSystemAI.Domain.Entities;

public sealed class Apartment
{
    public required Guid Id { get; init; }
    public required string HostId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required decimal PricePerNight { get; init; }
    public required int GuestCount { get; init; }
    public IReadOnlyList<Amenity> Amenities { get; init; } = [];
    public string MetadataJson { get; init; } = "{}";
    public Guid? SourceCompanyId { get; init; }
    public string? ExternalId { get; init; }
    public int Version { get; init; } = 1;
}
