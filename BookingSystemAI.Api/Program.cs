using BookingSystemAI.Api.Endpoints;
using BookingSystemAI.Api.OpenApi;
using BookingSystemAI.Application;
using BookingSystemAI.Infrastructure;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://[::1]:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<OpenApiDocumentInfoTransformer>();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
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

if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
    app.MapGet("/", () => Results.Redirect("http://127.0.0.1:5173"));
}

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapApartmentEndpoints();
app.MapHostApartmentEndpoints();
app.MapBookingEndpoints();
app.MapAnalyticsEndpoints();

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
