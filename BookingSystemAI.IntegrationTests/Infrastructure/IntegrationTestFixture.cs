using BookingSystemAI.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace BookingSystemAI.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("docker.io/library/postgres:16-alpine")
        .WithDatabase("booking_test")
        .WithUsername("booking")
        .WithPassword("booking_test_password")
        .Build();

    private HttpClient? _client;

    public HttpClient Client => _client ?? throw new InvalidOperationException("Test fixture is not initialized.");

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _client = CreateClient();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _client?.Dispose();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
    }
}
