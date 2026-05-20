namespace BookingSystemAI.Application;

public static class ApplicationRoles
{
    public const string Host = "Host";
    public const string Client = "Client";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Host,
        Client
    };
}
