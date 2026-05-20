using System.Security.Claims;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAI.Api.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bookings")
            .WithTags("Bookings")
            .RequireAuthorization(ApplicationRoles.Client);

        group.MapPost("/", CreateBooking)
            .WithName("CreateBooking")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create booking (Client)";
                operation.Description =
                    "Books an apartment for [start, end) when available. Requires apartmentVersion from catalog. Returns 409 on overlap or stale version, 403 for Host role.";
                return operation;
            });

        group.MapGet("/", ListMyBookings)
            .WithName("ListMyBookings")
            .WithOpenApi(operation =>
            {
                operation.Summary = "List my bookings (Client)";
                operation.Description = "Returns bookings for the authenticated Client only.";
                return operation;
            });

        return group;
    }

    private static async Task<IResult> CreateBooking(
        CreateBookingRequestDto request,
        ClaimsPrincipal user,
        IBookingService bookingService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Results.Unauthorized();

        var result = await bookingService.CreateAsync(userId, request, cancellationToken);
        return MapCreateResult(result);
    }

    private static async Task<IResult> ListMyBookings(
        ClaimsPrincipal user,
        IBookingService bookingService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Results.Unauthorized();

        var bookings = await bookingService.ListMineAsync(userId, cancellationToken);
        return Results.Ok(bookings);
    }

    private static IResult MapCreateResult(CreateBookingResult result)
    {
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);

        return result.FailureReason switch
        {
            CreateBookingFailureReason.NotFound => Results.NotFound(),
            CreateBookingFailureReason.Conflict => Results.Conflict(),
            CreateBookingFailureReason.ApartmentVersionConflict => Results.Conflict(new
            {
                code = "apartmentUpdatedByHost",
                message =
                    "This apartment was updated by the host. Review the current listing and create your booking again."
            }),
            _ => Results.Created($"/bookings/{result.Response!.Id}", result.Response)
        };
    }

    private static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(ClaimTypes.Name);
}
