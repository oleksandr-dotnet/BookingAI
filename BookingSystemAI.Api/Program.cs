using BookingSystemAI.Api.Cors;
using BookingSystemAI.Api.Endpoints;
using BookingSystemAI.Api.OpenApi;
using BookingSystemAI.Application;
using BookingSystemAI.Infrastructure;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<OpenApiDocumentInfoTransformer>();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

ApiCorsExtensions.ValidateHostedCors(app.Environment, app.Configuration);

var corsPolicyName = ApiCorsExtensions.ResolveCorsPolicyName(app.Environment, app.Configuration);
var corsOrigins = ApiCorsExtensions.GetAllowedOrigins(app.Configuration);

if (corsPolicyName is not null)
{
    app.Logger.LogInformation(
        "CORS enabled: policy={Policy}, origins={Origins}",
        corsPolicyName,
        string.Join(", ", corsOrigins));
}
else
{
    app.Logger.LogWarning("CORS middleware is disabled (no allowed origins configured).");
}

if (app.Environment.IsDevelopment()
    || app.Environment.IsEnvironment("Testing")
    || app.Environment.IsStaging())
{
    await app.Services.MigrateDatabaseAsync();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Booking System API")
                .WithOpenApiRoutePattern("/openapi/{documentName}.json");
        });
    }
}

app.UseApiCors(corsPolicyName);

if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("http://127.0.0.1:5173"));

var isHostedTestDeploy = corsPolicyName == ApiCorsExtensions.DeployedPolicy;
if (!app.Environment.IsEnvironment("Testing") && !isHostedTestDeploy)
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints().WithApiCors(corsPolicyName);
app.MapApartmentEndpoints().WithApiCors(corsPolicyName);
app.MapHostApartmentEndpoints().WithApiCors(corsPolicyName);
app.MapBookingEndpoints().WithApiCors(corsPolicyName);
app.MapAnalyticsEndpoints().WithApiCors(corsPolicyName);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .RequireAuthorization()
    .WithApiCors(corsPolicyName)
    .WithOpenApi(operation =>
    {
        operation.Summary = "Get weather forecast";
        operation.Description = "Returns a 5-day forecast. Requires a valid JWT bearer token.";
        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] =
                    Array.Empty<string>()
            }
        ];
        return operation;
    });

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
