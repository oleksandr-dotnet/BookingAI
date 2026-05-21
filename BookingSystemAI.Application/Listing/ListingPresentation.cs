using System.Text.Json;

namespace BookingSystemAI.Application.Listing;

public static class ListingPresentation
{
    public static readonly string[] AllowedPropertyTypes = ["Apartment", "House", "Studio", "Room"];

    public const int MaxHighlights = 5;
    public const int MaxHighlightLength = 40;
    public const int MaxRoomCount = 20;

    public sealed record Projection(
        string? PropertyType,
        int? BedroomCount,
        int? BedCount,
        int? BathroomCount,
        IReadOnlyList<string>? Highlights);

    public static IReadOnlyDictionary<string, string[]>? Validate(JsonElement metadata)
    {
        var errors = new Dictionary<string, string[]>();

        if (metadata.TryGetProperty("propertyType", out var propertyTypeElement))
        {
            if (propertyTypeElement.ValueKind != JsonValueKind.String ||
                !AllowedPropertyTypes.Contains(propertyTypeElement.GetString()!, StringComparer.Ordinal))
            {
                errors["metadata"] =
                [
                    $"propertyType must be one of: {string.Join(", ", AllowedPropertyTypes)}."
                ];
            }
        }

        ValidateCount(metadata, "bedroomCount", errors);
        ValidateCount(metadata, "bedCount", errors);
        ValidateCount(metadata, "bathroomCount", errors);

        if (metadata.TryGetProperty("highlights", out var highlightsElement))
        {
            if (highlightsElement.ValueKind != JsonValueKind.Array)
            {
                errors["metadata"] = ["highlights must be an array of strings."];
            }
            else
            {
                var items = highlightsElement.EnumerateArray().ToList();
                if (items.Count > MaxHighlights)
                    errors["metadata"] = [$"highlights must contain at most {MaxHighlights} items."];

                foreach (var item in items)
                {
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        errors["metadata"] = ["highlights must be an array of strings."];
                        break;
                    }

                    var text = item.GetString()?.Trim() ?? string.Empty;
                    if (text.Length == 0 || text.Length > MaxHighlightLength)
                    {
                        errors["metadata"] =
                        [$"Each highlight must be between 1 and {MaxHighlightLength} characters."];
                        break;
                    }
                }
            }
        }

        return errors.Count == 0 ? null : errors;
    }

    public static Projection Project(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson) || metadataJson == "{}")
            return new(null, null, null, null, null);

        using var doc = JsonDocument.Parse(metadataJson);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return new(null, null, null, null, null);

        var propertyType = TryGetPropertyType(root);
        var bedroomCount = TryGetCount(root, "bedroomCount");
        var bedCount = TryGetCount(root, "bedCount");
        var bathroomCount = TryGetCount(root, "bathroomCount");
        var highlights = TryGetHighlights(root);

        if (propertyType is null && bedroomCount is null && bedCount is null && bathroomCount is null &&
            highlights is null)
            return new(null, null, null, null, null);

        return new(propertyType, bedroomCount, bedCount, bathroomCount, highlights);
    }

    public static JsonElement? BuildMetadataElement(
        string? propertyType,
        int? bedroomCount,
        int? bedCount,
        int? bathroomCount,
        IReadOnlyList<string>? highlights,
        JsonElement? existingMetadata)
    {
        var dict = new Dictionary<string, object?>();

        if (existingMetadata is { ValueKind: JsonValueKind.Object } existing)
        {
            foreach (var prop in existing.EnumerateObject())
            {
                if (prop.Name is "propertyType" or "bedroomCount" or "bedCount" or "bathroomCount" or "highlights")
                    continue;
                dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
            }
        }

        if (!string.IsNullOrWhiteSpace(propertyType))
            dict["propertyType"] = propertyType;
        if (bedroomCount.HasValue)
            dict["bedroomCount"] = bedroomCount.Value;
        if (bedCount.HasValue)
            dict["bedCount"] = bedCount.Value;
        if (bathroomCount.HasValue)
            dict["bathroomCount"] = bathroomCount.Value;
        if (highlights is { Count: > 0 })
            dict["highlights"] = highlights;

        if (dict.Count == 0)
            return null;

        var json = JsonSerializer.Serialize(dict);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static void ValidateCount(JsonElement metadata, string key, Dictionary<string, string[]> errors)
    {
        if (!metadata.TryGetProperty(key, out var element))
            return;

        if (element.ValueKind != JsonValueKind.Number || !element.TryGetInt32(out var value) || value < 0 ||
            value > MaxRoomCount)
            errors["metadata"] = [$"{key} must be an integer from 0 to {MaxRoomCount}."];
    }

    private static string? TryGetPropertyType(JsonElement root)
    {
        if (!root.TryGetProperty("propertyType", out var element) || element.ValueKind != JsonValueKind.String)
            return null;

        var value = element.GetString();
        return value is not null && AllowedPropertyTypes.Contains(value, StringComparer.Ordinal) ? value : null;
    }

    private static int? TryGetCount(JsonElement root, string key)
    {
        if (!root.TryGetProperty(key, out var element) || element.ValueKind != JsonValueKind.Number ||
            !element.TryGetInt32(out var value) || value < 0 || value > MaxRoomCount)
            return null;

        return value;
    }

    private static IReadOnlyList<string>? TryGetHighlights(JsonElement root)
    {
        if (!root.TryGetProperty("highlights", out var element) || element.ValueKind != JsonValueKind.Array)
            return null;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<string>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
                continue;

            var text = item.GetString()?.Trim() ?? string.Empty;
            if (text.Length == 0 || text.Length > MaxHighlightLength || !seen.Add(text))
                continue;

            list.Add(text);
            if (list.Count >= MaxHighlights)
                break;
        }

        return list.Count == 0 ? null : list;
    }
}
