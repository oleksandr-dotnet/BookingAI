namespace BookingSystemAI.Infrastructure.Sql;

internal sealed class ApartmentUpsertRowDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int GuestCount { get; set; }
    public string[] Amenities { get; set; } = [];
    public required string MetadataJson { get; set; }
}

internal sealed class BookingSummaryRowDto
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePricePerNight { get; set; }
}

internal sealed class BookingsByApartmentRowDto
{
    public Guid ApartmentId { get; set; }
    public int BookingCount { get; set; }
    public decimal RevenueSum { get; set; }
}

internal sealed class ActiveHostRowDto
{
    public required string HostId { get; set; }
    public int BookingCount { get; set; }
}

internal sealed class PriceQuantilesRowDto
{
    public double? P25 { get; set; }
    public double? P50 { get; set; }
    public double? P75 { get; set; }
}

internal sealed class ApartmentOccupancyRowDto
{
    public Guid ApartmentId { get; set; }
    public int BookingCount { get; set; }
    public double AverageNightsBooked { get; set; }
}
