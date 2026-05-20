namespace BookingSystemAI.Application.Migration;

public sealed class CompanyMigrationResult
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }
    public int HostsImported { get; init; }
    public int ClientsImported { get; init; }
    public int ApartmentsImported { get; init; }

    public static CompanyMigrationResult Success(int hosts, int clients, int apartments) =>
        new()
        {
            Succeeded = true,
            HostsImported = hosts,
            ClientsImported = clients,
            ApartmentsImported = apartments
        };

    public static CompanyMigrationResult Failure(string error) =>
        new() { Succeeded = false, Error = error };
}
