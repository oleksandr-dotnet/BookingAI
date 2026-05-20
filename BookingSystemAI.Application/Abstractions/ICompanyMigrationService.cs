using BookingSystemAI.Application.Migration;

namespace BookingSystemAI.Application.Abstractions;

public interface ICompanyMigrationService
{
    Task<CompanyMigrationResult> MigrateAsync(string exportFilePath, string defaultPassword,
        CancellationToken cancellationToken = default);
}
