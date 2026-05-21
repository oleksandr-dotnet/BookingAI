using System.Text.Json;
using BookingSystemAI.Domain;

namespace BookingSystemAI.Application.Listing;

public static class ListingValidation
{
    public const int MaxMetadataBytes = 16 * 1024;

    public static IReadOnlyDictionary<string, string[]>? ValidateCity(string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return new Dictionary<string, string[]>
            {
                ["city"] = ["City is required."]
            };
        }

        var trimmed = city.Trim();
        if (trimmed.Length > ApartmentListingFields.MaxCityLength)
        {
            return new Dictionary<string, string[]>
            {
                ["city"] = [$"City must not exceed {ApartmentListingFields.MaxCityLength} characters."]
            };
        }

        return null;
    }

    public static IReadOnlyDictionary<string, string[]>? ValidateImageUrlCount(IReadOnlyList<string>? imageUrls)
    {
        if (imageUrls is null || imageUrls.Count == 0)
            return null;

        if (imageUrls.Count > ApartmentListingFields.MaxImageCount)
        {
            return new Dictionary<string, string[]>
            {
                ["imageUrls"] = [$"At most {ApartmentListingFields.MaxImageCount} images are allowed."]
            };
        }

        foreach (var imageUrl in imageUrls)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                continue;

            if (imageUrl.Trim().Length > ApartmentListingFields.MaxImageUrlLength)
            {
                return new Dictionary<string, string[]>
                {
                    ["imageUrls"] = [$"Each image URL must not exceed {ApartmentListingFields.MaxImageUrlLength} characters."]
                };
            }
        }

        return null;
    }

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

        return ListingPresentation.Validate(metadata.Value);
    }

    public static string SerializeMetadata(JsonElement? metadata) =>
        metadata?.GetRawText() ?? "{}";
}
