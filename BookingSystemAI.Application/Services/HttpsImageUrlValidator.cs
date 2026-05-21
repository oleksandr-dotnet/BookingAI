using BookingSystemAI.Application.Abstractions;

namespace BookingSystemAI.Application.Services;

public sealed class HttpsImageUrlValidator : IImageUrlValidator
{
    private const int MaxUrlLength = 2048;

    public IReadOnlyDictionary<string, string[]>? ValidateUrls(IReadOnlyList<string>? imageUrls)
    {
        if (imageUrls is null || imageUrls.Count == 0)
            return null;

        var errors = new List<string>();
        foreach (var imageUrl in imageUrls)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                continue;

            var trimmed = imageUrl.Trim();
            if (trimmed.Length > MaxUrlLength)
                errors.Add($"Image URL must be at most {MaxUrlLength} characters.");
            else if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                errors.Add($"Image URL must be an absolute HTTPS URL: '{trimmed}'.");
        }

        return errors.Count == 0
            ? null
            : new Dictionary<string, string[]> { ["profileImageUrl"] = errors.ToArray() };
    }
}
