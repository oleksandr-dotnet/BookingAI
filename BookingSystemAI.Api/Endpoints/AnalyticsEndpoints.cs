using BookingSystemAI.Application;
using BookingSystemAI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAI.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static RouteGroupBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/analytics")
            .WithTags("Analytics")
            .RequireAuthorization(ApplicationRoles.Admin);

        group.MapGet("/bookings/summary", GetBookingSummary)
            .WithName("GetBookingSummaryAnalytics")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Booking aggregate summary (Admin)";
                operation.Description = "COUNT, SUM, and AVG over all bookings via SQL aggregates.";
                return operation;
            });

        group.MapGet("/bookings/by-apartment", GetBookingsByApartment)
            .WithName("GetBookingsByApartmentAnalytics")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Bookings grouped by apartment (Admin)";
                operation.Description = "Per-apartment booking count and revenue using GROUP BY.";
                return operation;
            });

        group.MapGet("/hosts/active", GetActiveHosts)
            .WithName("GetActiveHostsAnalytics")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Active hosts by booking count (Admin)";
                operation.Description = "Hosts with at least minBookings reservations (HAVING). Default minBookings=1.";
                return operation;
            });

        group.MapGet("/apartments/price-quantiles", GetPriceQuantiles)
            .WithName("GetPriceQuantilesAnalytics")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Apartment price quantiles (Admin)";
                operation.Description = "p25, p50, p75 of pricePerNight using percentile_cont.";
                return operation;
            });

        group.MapGet("/apartments/occupancy", GetApartmentOccupancy)
            .WithName("GetApartmentOccupancyAnalytics")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Apartment occupancy summary (Admin)";
                operation.Description = "Average nights booked per apartment with optional minAvgNights HAVING filter.";
                return operation;
            });

        return group;
    }

    private static async Task<IResult> GetBookingSummary(
        IApartmentAnalyticsService analyticsService,
        CancellationToken cancellationToken) =>
        Results.Ok(await analyticsService.GetBookingSummaryAsync(cancellationToken));

    private static async Task<IResult> GetBookingsByApartment(
        IApartmentAnalyticsService analyticsService,
        CancellationToken cancellationToken) =>
        Results.Ok(await analyticsService.GetBookingsByApartmentAsync(cancellationToken));

    private static async Task<IResult> GetActiveHosts(
        IApartmentAnalyticsService analyticsService,
        [FromQuery] int? minBookings,
        CancellationToken cancellationToken)
    {
        var min = minBookings ?? 1;
        if (min < 1)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["minBookings"] = ["minBookings must be at least 1."]
            });

        return Results.Ok(await analyticsService.GetActiveHostsAsync(min, cancellationToken));
    }

    private static async Task<IResult> GetPriceQuantiles(
        IApartmentAnalyticsService analyticsService,
        CancellationToken cancellationToken) =>
        Results.Ok(await analyticsService.GetPriceQuantilesAsync(cancellationToken));

    private static async Task<IResult> GetApartmentOccupancy(
        IApartmentAnalyticsService analyticsService,
        [FromQuery] decimal? minAvgNights,
        CancellationToken cancellationToken) =>
        Results.Ok(await analyticsService.GetApartmentOccupancyAsync(minAvgNights ?? 0, cancellationToken));
}
