namespace BookingSystemAI.Infrastructure.Data.Entities;

public sealed class ApartmentRecord
{
    public Guid Id { get; set; }
    public required string HostId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public Guid? SourceCompanyId { get; set; }
    public string? ExternalId { get; set; }
    public ICollection<BookingRecord> Bookings { get; set; } = [];
}
