using BookingSystemAI.Domain;

namespace BookingSystemAI.Application.Listing;

public static class AmenityMapper
{
    public static IReadOnlyList<string> ToNames(IReadOnlyList<Amenity> amenities) =>
        amenities.Select(a => a.ToString()).ToList();

    public static IReadOnlyList<Amenity> FromNames(IReadOnlyList<string>? names)
    {
        if (names is null || names.Count == 0)
            return [];

        return names.Select(Parse).ToList();
    }

    public static Amenity Parse(string name)
    {
        if (Enum.TryParse<Amenity>(name, ignoreCase: false, out var amenity))
            return amenity;

        throw new ArgumentException($"Unknown amenity '{name}'.", nameof(name));
    }
}
