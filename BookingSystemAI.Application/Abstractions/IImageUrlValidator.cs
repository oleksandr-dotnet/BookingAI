namespace BookingSystemAI.Application.Abstractions;

public interface IImageUrlValidator
{
    IReadOnlyDictionary<string, string[]>? ValidateUrls(IReadOnlyList<string>? imageUrls);
}
