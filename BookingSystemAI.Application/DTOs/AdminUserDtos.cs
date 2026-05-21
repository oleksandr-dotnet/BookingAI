namespace BookingSystemAI.Application.DTOs;

public sealed record AdminUserListItemDto(
    string UserId,
    string Email,
    string? UserName,
    IReadOnlyList<string> Roles,
    bool EmailConfirmed,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    int BookingCount,
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? ProfileImageUrl,
    string DisplayName);

public sealed record AdminUserDetailDto(
    string UserId,
    string Email,
    string? UserName,
    IReadOnlyList<string> Roles,
    bool EmailConfirmed,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    Guid? SourceCompanyId,
    string? ExternalId,
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? ProfileImageUrl,
    string DisplayName);

public sealed record AdminUserListQuery(
    string? Role,
    string? Search,
    int Page = 1,
    int PageSize = 20,
    string Sort = "email");

public sealed record AdminUserListResponseDto(
    IReadOnlyList<AdminUserListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
