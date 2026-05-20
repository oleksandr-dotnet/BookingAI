using BookingSystemAI.Domain;
using BookingSystemAI.Domain.Entities;
using BookingSystemAI.Infrastructure.Data.Entities;

namespace BookingSystemAI.Infrastructure.Sql;

internal static class EntityMapping
{
    public static Apartment MapToDomain(ApartmentRecord record) =>
        new()
        {
            Id = record.Id,
            HostId = record.HostId,
            Name = record.Name,
            Description = record.Description,
            PricePerNight = record.PricePerNight,
            GuestCount = record.GuestCount,
            Amenities = MapAmenities(record.Amenities),
            MetadataJson = record.MetadataJson,
            SourceCompanyId = record.SourceCompanyId,
            ExternalId = record.ExternalId,
            Version = record.Version
        };

    public static Booking MapToDomain(BookingRecord record) =>
        new()
        {
            Id = record.Id,
            ApartmentId = record.ApartmentId,
            UserId = record.UserId,
            Start = record.Start,
            End = record.End,
            PricePerNight = record.PricePerNight,
            GuestCount = record.GuestCount,
            Amenities = MapAmenities(record.Amenities)
        };

    public static List<string> MapAmenityNames(IReadOnlyList<Amenity> amenities) =>
        amenities.Select(a => a.ToString()).ToList();

    private static IReadOnlyList<Amenity> MapAmenities(IReadOnlyList<string> names) =>
        names.Select(n => Enum.Parse<Amenity>(n)).ToList();
}
