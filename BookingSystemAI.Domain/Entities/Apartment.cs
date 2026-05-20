namespace BookingSystemAI.Domain.Entities;

public sealed class Apartment
{
    public required Guid Id { get; init; }
    public required string HostId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public Guid? SourceCompanyId { get; init; }
    public string? ExternalId { get; init; }
}
