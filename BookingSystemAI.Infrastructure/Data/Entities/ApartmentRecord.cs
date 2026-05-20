namespace BookingSystemAI.Infrastructure.Data.Entities;

public sealed class ApartmentRecord
{
    public Guid Id { get; set; }
    public required string HostId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int GuestCount { get; set; }
    public List<string> Amenities { get; set; } = [];
    public string MetadataJson { get; set; } = "{}";
    public Guid? SourceCompanyId { get; set; }
    public string? ExternalId { get; set; }
    public int Version { get; set; } = 1;
    public ICollection<BookingRecord> Bookings { get; set; } = [];
}
