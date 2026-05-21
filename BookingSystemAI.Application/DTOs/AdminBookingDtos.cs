namespace BookingSystemAI.Application.DTOs;

public sealed record AdminBookingListItemDto(
    Guid BookingId,
    string UserId,
    string UserEmail,
    Guid ApartmentId,
    string ApartmentName,
    string City,
    DateTimeOffset Start,
    DateTimeOffset End,
    decimal PricePerNight);

public sealed record AdminBookingListQuery(
    string? UserId,
    int Page = 1,
    int PageSize = 20);

public sealed record AdminBookingListResponseDto(
    IReadOnlyList<AdminBookingListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record SetUserRolesRequestDto(IReadOnlyList<string> Roles);
