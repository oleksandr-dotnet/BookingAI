using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public interface IBookingService
{
    Task<CreateBookingResult> CreateAsync(string userId, CreateBookingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingResponseDto>> ListMineAsync(string userId,
        CancellationToken cancellationToken = default);
}
