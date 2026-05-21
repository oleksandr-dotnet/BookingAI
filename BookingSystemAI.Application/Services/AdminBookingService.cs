using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public class AdminBookingService(IAdminBookingQuery adminBookingQuery) : IAdminBookingService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<AdminBookingListResult> ListBookingsAsync(
        AdminBookingListQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateQuery(query);
        if (validationErrors is not null)
            return AdminBookingListResult.ValidationFailure(validationErrors);

        var normalized = NormalizeQuery(query);
        var (items, totalCount) = await adminBookingQuery.ListAsync(normalized, cancellationToken);
        return AdminBookingListResult.Success(new PagedResult<AdminBookingListItemDto>
        {
            Items = items,
            Page = normalized.Page,
            PageSize = normalized.PageSize,
            TotalCount = totalCount
        });
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateQuery(AdminBookingListQuery query)
    {
        var errors = new Dictionary<string, string[]>();
        if (query.Page < 1)
            errors["page"] = ["Page must be at least 1."];
        if (query.PageSize > MaxPageSize)
            errors["pageSize"] = [$"PageSize must be at most {MaxPageSize}."];
        return errors.Count > 0 ? errors : null;
    }

    private static AdminBookingListQuery NormalizeQuery(AdminBookingListQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize
        };
        return query with
        {
            Page = page,
            PageSize = pageSize,
            UserId = string.IsNullOrWhiteSpace(query.UserId) ? null : query.UserId.Trim()
        };
    }
}
