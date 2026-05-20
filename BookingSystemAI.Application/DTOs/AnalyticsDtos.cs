namespace BookingSystemAI.Application.DTOs;

public record BookingSummaryAnalyticsDto(int TotalBookings, decimal TotalRevenue, decimal AveragePricePerNight);

public record BookingsByApartmentAnalyticsDto(Guid ApartmentId, int BookingCount, decimal RevenueSum);

public record ActiveHostAnalyticsDto(string HostId, int BookingCount);

public record PriceQuantilesAnalyticsDto(decimal? P25, decimal? P50, decimal? P75);

public record ApartmentOccupancyAnalyticsDto(Guid ApartmentId, int BookingCount, decimal AverageNightsBooked);
