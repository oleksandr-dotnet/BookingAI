using System.Text.Json;
using BookingSystemAI.Domain;

namespace BookingSystemAI.Application.Listing;

public static class ListingValidation
{
    public const int MaxMetadataBytes = 16 * 1024;

    public static IReadOnlyDictionary<string, string[]>? ValidateEconomics(decimal pricePerNight, int guestCount)
    {
        var errors = new Dictionary<string, string[]>();

        if (pricePerNight < 0)
            errors["pricePerNight"] = ["Price per night cannot be negative."];

        if (guestCount < 1)
            errors["guestCount"] = ["Guest count must be at least 1."];

        return errors.Count == 0 ? null : errors;
    }

    public static IReadOnlyDictionary<string, string[]>? ValidateAmenities(IReadOnlyList<string>? amenityNames)
    {
        if (amenityNames is null || amenityNames.Count == 0)
            return null;

        var errors = new Dictionary<string, string[]>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var name in amenityNames)
        {
            if (!Enum.TryParse<Amenity>(name, ignoreCase: false, out _))
                errors["amenities"] = [$"Unknown amenity '{name}'. Allowed: LargeBed, Microwave, Bath, Shower."];
            else
                seen.Add(name);
        }

        return errors.Count == 0 ? null : errors;
    }

    public static IReadOnlyList<string> DeduplicateAmenityNames(IReadOnlyList<string>? amenityNames)
    {
        if (amenityNames is null || amenityNames.Count == 0)
            return [];

        return amenityNames.Distinct(StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyDictionary<string, string[]>? ValidateMetadata(JsonElement? metadata)
    {
        if (metadata is null)
            return null;

        var json = metadata.Value.GetRawText();
        if (json.Length > MaxMetadataBytes)
        {
            return new Dictionary<string, string[]>
            {
                ["metadata"] = ["Metadata must not exceed 16 KB."]
            };
        }

        if (metadata.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new Dictionary<string, string[]>
            {
                ["metadata"] = ["Metadata must be a JSON object."]
            };
        }

        if (metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, string[]>
            {
                ["metadata"] = ["Metadata must be a JSON object."]
            };
        }

        return null;
    }

    public static string SerializeMetadata(JsonElement? metadata) =>
        metadata?.GetRawText() ?? "{}";
}
