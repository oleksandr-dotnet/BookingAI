using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Services;

public interface IApartmentAnalyticsService
{
    Task<BookingSummaryAnalyticsDto> GetBookingSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingsByApartmentAnalyticsDto>> GetBookingsByApartmentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveHostAnalyticsDto>> GetActiveHostsAsync(int minBookings, CancellationToken cancellationToken = default);
    Task<PriceQuantilesAnalyticsDto> GetPriceQuantilesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApartmentOccupancyAnalyticsDto>> GetApartmentOccupancyAsync(decimal minAvgNights, CancellationToken cancellationToken = default);
}
