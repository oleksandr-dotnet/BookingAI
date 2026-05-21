using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAI.Api.Endpoints;

public static class AdminBookingEndpoints
{
    public static RouteGroupBuilder MapAdminBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/bookings")
            .WithTags("Admin Bookings")
            .RequireAuthorization(ApplicationRoles.Admin);

        group.MapGet("/", ListBookings)
            .WithName("ListAdminBookings")
            .WithOpenApi(operation =>
            {
                operation.Summary = "List all bookings (Admin)";
                operation.Description = "Paginated bookings across users with optional userId filter.";
                return operation;
            });

        return group;
    }

    private static async Task<IResult> ListBookings(
        IAdminBookingService adminBookingService,
        [FromQuery] string? userId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var query = new AdminBookingListQuery(userId, page ?? 1, pageSize ?? 20);
        var result = await adminBookingService.ListBookingsAsync(query, cancellationToken);
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);

        var paged = result.Response!;
        return Results.Ok(new AdminBookingListResponseDto(
            paged.Items,
            paged.Page,
            paged.PageSize,
            paged.TotalCount));
    }
}
