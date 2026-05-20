using BookingSystemAI.Infrastructure.CompanyImport;
using Shouldly;

namespace BookingSystemAI.Application.Tests.Migration;

public class JsonExportReaderTests
{
    [Fact]
    public async Task ReadAsync_ShouldParseValidExport_WhenSchemaMatches()
    {
        var path = WriteTempJson(
            """
            {
              "sourceCompanyId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
              "hosts": [
                {
                  "externalId": "host-1",
                  "email": "host@test.com",
                  "apartments": [
                    { "externalId": "apt-1", "name": "A", "description": "D" }
                  ]
                }
              ],
              "clients": [
                { "externalId": "client-1", "email": "client@test.com" }
              ]
            }
            """);

        var reader = new JsonExportReader();
        var export = await reader.ReadAsync(path);

        export.SourceCompanyId.ShouldBe(Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"));
        export.Hosts.Count.ShouldBe(1);
        export.Hosts[0].Apartments.Count.ShouldBe(1);
        export.Clients.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ReadAsync_ShouldThrow_WhenSourceCompanyIdMissing()
    {
        var path = WriteTempJson("""{ "hosts": [], "clients": [] }""");
        var reader = new JsonExportReader();

        await Should.ThrowAsync<InvalidDataException>(() => reader.ReadAsync(path));
    }

    private static string WriteTempJson(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"export-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }
}
