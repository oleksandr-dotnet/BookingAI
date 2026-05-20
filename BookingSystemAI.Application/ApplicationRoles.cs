namespace BookingSystemAI.Application;

public static class ApplicationRoles
{
    public const string Host = "Host";
    public const string Client = "Client";
    public const string Admin = "Admin";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Host,
        Client,
        Admin
    };

    public static readonly IReadOnlySet<string> Registerable = new HashSet<string>(StringComparer.Ordinal)
    {
        Host,
        Client
    };
}
