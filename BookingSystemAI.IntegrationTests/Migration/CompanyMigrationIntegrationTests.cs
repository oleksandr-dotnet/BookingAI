using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Migration;
using BookingSystemAI.Infrastructure.Data;
using BookingSystemAI.Infrastructure.Data.Entities;
using BookingSystemAI.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace BookingSystemAI.IntegrationTests.Migration;

[Collection(IntegrationTestCollection.Name)]
public class CompanyMigrationIntegrationTests(IntegrationTestFixture fixture)
{
    private const string DefaultPassword = "TempPass123!";
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task MigrateAsync_ShouldImportHostsClientsAndApartments_WhenSampleExportIsValid()
    {
        var export = await CreateExportFileAsync();
        await using var scope = fixture.Services.CreateAsyncScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<ICompanyMigrationService>();

        var result = await migrationService.MigrateAsync(export.FilePath, DefaultPassword);

        result.Succeeded.ShouldBeTrue();
        result.HostsImported.ShouldBe(1);
        result.ClientsImported.ShouldBe(1);
        result.ApartmentsImported.ShouldBe(2);

        var hostLogin = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequestDto(export.HostEmail, DefaultPassword));
        hostLogin.StatusCode.ShouldBe(HttpStatusCode.OK);

        var hostToken = (await hostLogin.Content.ReadFromJsonAsync<AuthResponseDto>())!.AccessToken;
        var apartmentsResponse = await SendAuthorizedAsync(HttpMethod.Get, "/host/apartments", hostToken);
        apartmentsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var apartments = await apartmentsResponse.Content.ReadFromJsonAsync<List<ApartmentResponseDto>>();
        apartments!.Count.ShouldBe(2);

        var clientLogin = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequestDto(export.ClientEmail, DefaultPassword));
        clientLogin.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MigrateAsync_ShouldRollback_WhenDuplicateExternalIdExists()
    {
        var export = await CreateExportFileAsync();
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var migrationService = scope.ServiceProvider.GetRequiredService<ICompanyMigrationService>();

        db.Apartments.Add(new ApartmentRecord
        {
            Id = Guid.NewGuid(),
            HostId = "rollback-host",
            Name = "Existing",
            Description = "Blocks migration",
            SourceCompanyId = export.SourceCompanyId,
            ExternalId = export.FirstApartmentExternalId
        });
        await db.SaveChangesAsync();

        var result = await migrationService.MigrateAsync(export.FilePath, DefaultPassword);

        result.Succeeded.ShouldBeFalse();
        (await db.Apartments.CountAsync(a => a.HostId == "rollback-host")).ShouldBe(1);
        (await db.Users.CountAsync(u => u.Email == export.HostEmail || u.Email == export.ClientEmail)).ShouldBe(0);
    }

    private async Task<ExportFixture> CreateExportFileAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var companyId = Guid.NewGuid();
        var hostEmail = $"host-{suffix}@acquired.test";
        var clientEmail = $"client-{suffix}@acquired.test";
        var apt1 = $"apt-{suffix}-1";
        var apt2 = $"apt-{suffix}-2";
        var path = Path.Combine(Path.GetTempPath(), $"migration-{suffix}.json");
        var json =
            $$"""
              {
                "sourceCompanyId": "{{companyId}}",
                "hosts": [
                  {
                    "externalId": "host-{{suffix}}",
                    "email": "{{hostEmail}}",
                    "apartments": [
                      {
                        "externalId": "{{apt1}}",
                        "name": "Riverside Studio",
                        "description": "Bright studio apartment overlooking the river."
                      },
                      {
                        "externalId": "{{apt2}}",
                        "name": "Garden Flat",
                        "description": "Ground-floor flat with private garden access."
                      }
                    ]
                  }
                ],
                "clients": [
                  {
                    "externalId": "client-{{suffix}}",
                    "email": "{{clientEmail}}"
                  }
                ]
              }
              """;
        await File.WriteAllTextAsync(path, json);
        return new ExportFixture(companyId, hostEmail, clientEmail, apt1, path);
    }

    private Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string url, string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return _client.SendAsync(request);
    }

    private sealed record ExportFixture(
        Guid SourceCompanyId,
        string HostEmail,
        string ClientEmail,
        string FirstApartmentExternalId,
        string FilePath);
}
