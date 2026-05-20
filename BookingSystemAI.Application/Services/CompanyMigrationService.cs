using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.Migration;
using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Services;

public sealed class CompanyMigrationService(
    IJsonExportReader jsonExportReader,
    IMigrationTransactor migrationTransactor,
    IIdentityUserManager identityUserManager,
    IExternalEntityLookup externalEntityLookup,
    IMigratedApartmentWriter migratedApartmentWriter) : ICompanyMigrationService
{
    public async Task<CompanyMigrationResult> MigrateAsync(string exportFilePath, string defaultPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(exportFilePath))
            return CompanyMigrationResult.Failure("Export file path is required.");

        if (!File.Exists(exportFilePath))
            return CompanyMigrationResult.Failure($"Export file not found: {exportFilePath}");

        if (string.IsNullOrWhiteSpace(defaultPassword))
            return CompanyMigrationResult.Failure("Default password is required.");

        CompanyExportDto export;
        try
        {
            export = await jsonExportReader.ReadAsync(exportFilePath, cancellationToken);
        }
        catch (Exception ex)
        {
            return CompanyMigrationResult.Failure($"Failed to parse export file: {ex.Message}");
        }

        var validationError = ValidateExport(export);
        if (validationError is not null)
            return CompanyMigrationResult.Failure(validationError);

        var hostsImported = 0;
        var clientsImported = 0;
        var apartmentsImported = 0;

        try
        {
            await migrationTransactor.ExecuteInTransactionAsync(async ct =>
            {
                var hostIdByExternalId = new Dictionary<string, string>(StringComparer.Ordinal);

                foreach (var host in export.Hosts)
                {
                    if (await externalEntityLookup.UserExistsAsync(export.SourceCompanyId, host.ExternalId, ct))
                        throw new InvalidOperationException(
                            $"User with external id '{host.ExternalId}' already exists for company '{export.SourceCompanyId}'.");

                    var createResult = await identityUserManager.CreateMigratedUserAsync(
                        host.Email,
                        defaultPassword,
                        ApplicationRoles.Host,
                        export.SourceCompanyId,
                        host.ExternalId,
                        ct);

                    if (!createResult.Succeeded)
                        throw new InvalidOperationException(FormatIdentityErrors(createResult.Errors));

                    hostIdByExternalId[host.ExternalId] = createResult.UserId!;
                    hostsImported++;

                    foreach (var apartment in host.Apartments)
                    {
                        if (await externalEntityLookup.ApartmentExistsAsync(export.SourceCompanyId, apartment.ExternalId, ct))
                            throw new InvalidOperationException(
                                $"Apartment with external id '{apartment.ExternalId}' already exists for company '{export.SourceCompanyId}'.");

                        await migratedApartmentWriter.AddAsync(
                            new Apartment
                            {
                                Id = Guid.NewGuid(),
                                HostId = hostIdByExternalId[host.ExternalId],
                                Name = apartment.Name,
                                Description = apartment.Description,
                                SourceCompanyId = export.SourceCompanyId,
                                ExternalId = apartment.ExternalId
                            },
                            ct);

                        apartmentsImported++;
                    }
                }

                foreach (var client in export.Clients)
                {
                    if (await externalEntityLookup.UserExistsAsync(export.SourceCompanyId, client.ExternalId, ct))
                        throw new InvalidOperationException(
                            $"User with external id '{client.ExternalId}' already exists for company '{export.SourceCompanyId}'.");

                    var createResult = await identityUserManager.CreateMigratedUserAsync(
                        client.Email,
                        defaultPassword,
                        ApplicationRoles.Client,
                        export.SourceCompanyId,
                        client.ExternalId,
                        ct);

                    if (!createResult.Succeeded)
                        throw new InvalidOperationException(FormatIdentityErrors(createResult.Errors));

                    clientsImported++;
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return CompanyMigrationResult.Failure(ex.Message);
        }

        return CompanyMigrationResult.Success(hostsImported, clientsImported, apartmentsImported);
    }

    private static string? ValidateExport(CompanyExportDto export)
    {
        if (export.SourceCompanyId == Guid.Empty)
            return "sourceCompanyId must be a non-empty GUID.";

        if (export.Hosts.Count == 0 && export.Clients.Count == 0)
            return "Export must contain at least one host or client.";

        var userExternalIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var host in export.Hosts)
        {
            if (string.IsNullOrWhiteSpace(host.ExternalId) || string.IsNullOrWhiteSpace(host.Email))
                return "Each host must have externalId and email.";

            if (!userExternalIds.Add(host.ExternalId))
                return $"Duplicate host externalId '{host.ExternalId}' in export.";

            var apartmentExternalIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var apartment in host.Apartments)
            {
                if (string.IsNullOrWhiteSpace(apartment.ExternalId) ||
                    string.IsNullOrWhiteSpace(apartment.Name) ||
                    string.IsNullOrWhiteSpace(apartment.Description))
                    return $"Host '{host.ExternalId}' has an apartment with missing required fields.";

                if (!apartmentExternalIds.Add(apartment.ExternalId))
                    return $"Duplicate apartment externalId '{apartment.ExternalId}' under host '{host.ExternalId}'.";
            }
        }

        foreach (var client in export.Clients)
        {
            if (string.IsNullOrWhiteSpace(client.ExternalId) || string.IsNullOrWhiteSpace(client.Email))
                return "Each client must have externalId and email.";

            if (!userExternalIds.Add(client.ExternalId))
                return $"Duplicate user externalId '{client.ExternalId}' in export.";
        }

        return null;
    }

    private static string FormatIdentityErrors(IReadOnlyDictionary<string, string[]> errors) =>
        string.Join("; ", errors.SelectMany(e => e.Value.Select(v => $"{e.Key}: {v}")));
}
