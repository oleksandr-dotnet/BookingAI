namespace BookingSystemAI.Application.Listing;

public static class ValidationErrors
{
    public static void Merge(Dictionary<string, string[]> errors, IReadOnlyDictionary<string, string[]>? source)
    {
        if (source is null)
            return;

        foreach (var (key, value) in source)
            errors[key] = value;
    }
}
