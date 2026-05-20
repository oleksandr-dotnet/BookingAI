using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.Migration;
using BookingSystemAI.Application;
using BookingSystemAI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var filePath = GetArgValue(args, "--file");
var defaultPassword = GetArgValue(args, "--default-password")
    ?? Environment.GetEnvironmentVariable("MIGRATION_DEFAULT_PASSWORD");

if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(defaultPassword))
{
    Console.Error.WriteLine(
        "Usage: BookingSystemAI.Migrator --file <export.json> --default-password <password>");
    Console.Error.WriteLine("       MIGRATION_DEFAULT_PASSWORD environment variable is also accepted.");
    return 1;
}

var host = Host.CreateApplicationBuilder(args);
host.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false);
host.Services.AddApplication();
host.Services.AddInfrastructure(host.Configuration);

using var app = host.Build();
await app.Services.MigrateDatabaseAsync();

var migrationService = app.Services.GetRequiredService<ICompanyMigrationService>();
var result = await migrationService.MigrateAsync(filePath, defaultPassword);

if (!result.Succeeded)
{
    Console.Error.WriteLine($"Migration failed: {result.Error}");
    return 1;
}

Console.WriteLine(
    $"Migration succeeded. Hosts: {result.HostsImported}, Clients: {result.ClientsImported}, Apartments: {result.ApartmentsImported}");
return 0;

static string? GetArgValue(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            return args[i + 1];
    }

    return null;
}
