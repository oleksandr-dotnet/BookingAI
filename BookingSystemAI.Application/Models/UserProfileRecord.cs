namespace BookingSystemAI.Application.Models;

public sealed class UserProfileRecord
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public string? UserName { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? ProfileImageUrl { get; init; }
}
