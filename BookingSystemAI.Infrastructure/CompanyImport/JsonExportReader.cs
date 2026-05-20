using System.Text.Json;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.Migration;

namespace BookingSystemAI.Infrastructure.CompanyImport;

public sealed class JsonExportReader : IJsonExportReader
{
    public async Task<CompanyExportDto> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        if (!root.TryGetProperty("sourceCompanyId", out var sourceCompanyIdElement) ||
            !Guid.TryParse(sourceCompanyIdElement.GetString(), out var sourceCompanyId))
            throw new InvalidDataException("Missing or invalid sourceCompanyId.");

        return new CompanyExportDto
        {
            SourceCompanyId = sourceCompanyId,
            Hosts = ReadHosts(root),
            Clients = ReadClients(root)
        };
    }

    private static IReadOnlyList<HostExportEntryDto> ReadHosts(JsonElement root)
    {
        if (!root.TryGetProperty("hosts", out var hostsElement) || hostsElement.ValueKind != JsonValueKind.Array)
            return [];

        var hosts = new List<HostExportEntryDto>();
        foreach (var hostElement in hostsElement.EnumerateArray())
        {
            hosts.Add(new HostExportEntryDto
            {
                ExternalId = GetRequiredString(hostElement, "externalId"),
                Email = GetRequiredString(hostElement, "email"),
                Apartments = ReadApartments(hostElement)
            });
        }

        return hosts;
    }

    private static IReadOnlyList<ApartmentExportEntryDto> ReadApartments(JsonElement hostElement)
    {
        if (!hostElement.TryGetProperty("apartments", out var apartmentsElement) ||
            apartmentsElement.ValueKind != JsonValueKind.Array)
            return [];

        var apartments = new List<ApartmentExportEntryDto>();
        foreach (var apartmentElement in apartmentsElement.EnumerateArray())
        {
            apartments.Add(new ApartmentExportEntryDto
            {
                ExternalId = GetRequiredString(apartmentElement, "externalId"),
                Name = GetRequiredString(apartmentElement, "name"),
                Description = GetRequiredString(apartmentElement, "description")
            });
        }

        return apartments;
    }

    private static IReadOnlyList<ClientExportEntryDto> ReadClients(JsonElement root)
    {
        if (!root.TryGetProperty("clients", out var clientsElement) || clientsElement.ValueKind != JsonValueKind.Array)
            return [];

        var clients = new List<ClientExportEntryDto>();
        foreach (var clientElement in clientsElement.EnumerateArray())
        {
            clients.Add(new ClientExportEntryDto
            {
                ExternalId = GetRequiredString(clientElement, "externalId"),
                Email = GetRequiredString(clientElement, "email")
            });
        }

        return clients;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            throw new InvalidDataException($"Missing or invalid property '{propertyName}'.");

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDataException($"Property '{propertyName}' must not be empty.");

        return value;
    }
}
