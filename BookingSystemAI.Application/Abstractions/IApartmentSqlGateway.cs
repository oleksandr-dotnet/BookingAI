using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Abstractions;

public interface IApartmentSqlGateway
{
    Task<ApartmentUpsertRow?> UpsertApartmentAsync(ApartmentUpsertCommand command, CancellationToken cancellationToken = default);
    Task<BookingSummaryAnalyticsDto> GetBookingSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingsByApartmentAnalyticsDto>> GetBookingsByApartmentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveHostAnalyticsDto>> GetActiveHostsAsync(int minBookings, CancellationToken cancellationToken = default);
    Task<PriceQuantilesAnalyticsDto> GetPriceQuantilesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApartmentOccupancyAnalyticsDto>> GetApartmentOccupancyAsync(decimal minAvgNights, CancellationToken cancellationToken = default);
}

public sealed record ApartmentUpsertCommand(
    Guid Id,
    string HostId,
    string Name,
    string Description,
    decimal PricePerNight,
    int GuestCount,
    string[] Amenities,
    string MetadataJson,
    Guid? SourceCompanyId,
    string? ExternalId,
    bool UpdateMetadata);

public sealed record ApartmentUpsertRow(
    Guid Id,
    string Name,
    string Description,
    decimal PricePerNight,
    int GuestCount,
    string[] Amenities,
    string MetadataJson,
    int Version);
