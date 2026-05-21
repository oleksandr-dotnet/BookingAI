namespace BookingSystemAI.Application.Models;

public sealed class AdminUserRecord
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public string? UserName { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool LockoutEnabled { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public Guid? SourceCompanyId { get; init; }
    public string? ExternalId { get; init; }
    public int BookingCount { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? ProfileImageUrl { get; init; }
}
