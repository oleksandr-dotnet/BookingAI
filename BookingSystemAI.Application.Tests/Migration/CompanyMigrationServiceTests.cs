using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.Migration;
using BookingSystemAI.Application.Services;
using Moq;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Migration;

public class CompanyMigrationServiceTests
{
    private readonly Mock<IJsonExportReader> _jsonExportReader = new();
    private readonly Mock<IMigrationTransactor> _migrationTransactor = new();
    private readonly Mock<IIdentityUserManager> _identityUserManager = new();
    private readonly Mock<IExternalEntityLookup> _externalEntityLookup = new();
    private readonly Mock<IMigratedApartmentWriter> _migratedApartmentWriter = new();
    private readonly CompanyMigrationService _sut;

    public CompanyMigrationServiceTests()
    {
        _migrationTransactor
            .Setup(t => t.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (action, ct) => await action(ct));

        _sut = new CompanyMigrationService(
            _jsonExportReader.Object,
            _migrationTransactor.Object,
            _identityUserManager.Object,
            _externalEntityLookup.Object,
            _migratedApartmentWriter.Object);
    }

    [Fact]
    public async Task MigrateAsync_ShouldFail_WhenDuplicateExternalUserExists()
    {
        var companyId = Guid.NewGuid();
        _jsonExportReader.Setup(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompanyExportDto
            {
                SourceCompanyId = companyId,
                Hosts =
                [
                    new HostExportEntryDto
                    {
                        ExternalId = "host-1",
                        Email = "host@test.com",
                        Apartments = []
                    }
                ],
                Clients = []
            });

        _externalEntityLookup
            .Setup(l => l.UserExistsAsync(companyId, "host-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var file = CreateExistingFile();
        var result = await _sut.MigrateAsync(file, "TempPass123!");

        result.Succeeded.ShouldBeFalse();
        result.Error!.ShouldContain("host-1");
    }

    [Fact]
    public async Task MigrateAsync_ShouldSucceed_WhenExportIsValid()
    {
        var companyId = Guid.NewGuid();
        _jsonExportReader.Setup(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompanyExportDto
            {
                SourceCompanyId = companyId,
                Hosts =
                [
                    new HostExportEntryDto
                    {
                        ExternalId = "host-1",
                        Email = "host@test.com",
                        Apartments =
                        [
                            new ApartmentExportEntryDto
                            {
                                ExternalId = "apt-1",
                                Name = "Studio",
                                Description = "Nice"
                            }
                        ]
                    }
                ],
                Clients =
                [
                    new ClientExportEntryDto { ExternalId = "client-1", Email = "client@test.com" }
                ]
            });

        _externalEntityLookup
            .Setup(l => l.UserExistsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _externalEntityLookup
            .Setup(l => l.ApartmentExistsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityUserManager
            .Setup(m => m.CreateMigratedUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                companyId,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string email, string _, string role, Guid _, string extId, CancellationToken _) =>
                IdentityCreateUserResult.Success($"{role}-{extId}"));

        var file = CreateExistingFile();
        var result = await _sut.MigrateAsync(file, "TempPass123!");

        result.Succeeded.ShouldBeTrue();
        result.HostsImported.ShouldBe(1);
        result.ClientsImported.ShouldBe(1);
        result.ApartmentsImported.ShouldBe(1);
    }

    private static string CreateExistingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"export-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, "{}");
        return path;
    }
}
