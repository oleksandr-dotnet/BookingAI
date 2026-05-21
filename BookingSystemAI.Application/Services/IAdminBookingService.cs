using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public interface IAdminBookingService
{
    Task<AdminBookingListResult> ListBookingsAsync(AdminBookingListQuery query, CancellationToken cancellationToken = default);
}
