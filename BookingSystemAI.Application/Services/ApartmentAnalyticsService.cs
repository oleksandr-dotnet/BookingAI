using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Services;

public sealed class ApartmentAnalyticsService(IApartmentSqlGateway sqlGateway) : IApartmentAnalyticsService
{
    public Task<BookingSummaryAnalyticsDto> GetBookingSummaryAsync(CancellationToken cancellationToken = default) =>
        sqlGateway.GetBookingSummaryAsync(cancellationToken);

    public Task<IReadOnlyList<BookingsByApartmentAnalyticsDto>> GetBookingsByApartmentAsync(
        CancellationToken cancellationToken = default) =>
        sqlGateway.GetBookingsByApartmentAsync(cancellationToken);

    public Task<IReadOnlyList<ActiveHostAnalyticsDto>> GetActiveHostsAsync(int minBookings,
        CancellationToken cancellationToken = default) =>
        sqlGateway.GetActiveHostsAsync(minBookings, cancellationToken);

    public Task<PriceQuantilesAnalyticsDto> GetPriceQuantilesAsync(CancellationToken cancellationToken = default) =>
        sqlGateway.GetPriceQuantilesAsync(cancellationToken);

    public Task<IReadOnlyList<ApartmentOccupancyAnalyticsDto>> GetApartmentOccupancyAsync(decimal minAvgNights,
        CancellationToken cancellationToken = default) =>
        sqlGateway.GetApartmentOccupancyAsync(minAvgNights, cancellationToken);
}
