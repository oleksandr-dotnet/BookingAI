using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public sealed class AdminBookingListResult
{
    public PagedResult<AdminBookingListItemDto>? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public bool Succeeded => ValidationErrors is null;

    public static AdminBookingListResult Success(PagedResult<AdminBookingListItemDto> response) =>
        new() { Response = response };

    public static AdminBookingListResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { ValidationErrors = errors };
}
