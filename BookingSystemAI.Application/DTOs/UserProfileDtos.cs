namespace BookingSystemAI.Application.DTOs;

public sealed record UserProfileDto(
    string UserId,
    string Email,
    string? UserName,
    IReadOnlyList<string> Roles,
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? ProfileImageUrl,
    string DisplayName,
    string Initials,
    bool ProfileComplete);

public sealed record UpdateUserProfileRequestDto(
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? ProfileImageUrl);

public sealed record UserDisplayDto(
    string UserId,
    string DisplayName,
    string? ProfileImageUrl,
    string Initials);
