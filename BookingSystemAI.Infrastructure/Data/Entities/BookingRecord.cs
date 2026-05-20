namespace BookingSystemAI.Infrastructure.Data.Entities;

public sealed class BookingRecord
{
    public Guid Id { get; set; }
    public Guid ApartmentId { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public ApartmentRecord? Apartment { get; set; }
}
