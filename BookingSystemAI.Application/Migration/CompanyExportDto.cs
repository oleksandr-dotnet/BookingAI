namespace BookingSystemAI.Application.Migration;

public sealed class CompanyExportDto
{
    public required Guid SourceCompanyId { get; init; }
    public required IReadOnlyList<HostExportEntryDto> Hosts { get; init; }
    public required IReadOnlyList<ClientExportEntryDto> Clients { get; init; }
}

public sealed class HostExportEntryDto
{
    public required string ExternalId { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyList<ApartmentExportEntryDto> Apartments { get; init; }
}

public sealed class ClientExportEntryDto
{
    public required string ExternalId { get; init; }
    public required string Email { get; init; }
}

public sealed class ApartmentExportEntryDto
{
    public required string ExternalId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
}
