using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Abstractions;

public interface IAdminBookingQuery
{
    Task<(IReadOnlyList<AdminBookingListItemDto> Items, int TotalCount)> ListAsync(
        AdminBookingListQuery query,
        CancellationToken cancellationToken = default);
}
