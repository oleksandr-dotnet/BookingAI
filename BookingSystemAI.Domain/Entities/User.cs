namespace BookingSystemAI.Domain.Entities;

public sealed class User
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public Guid? SourceCompanyId { get; init; }
    public string? ExternalId { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}
