using System.Security.Claims;
using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAI.Api.Endpoints;

public static class HostApartmentEndpoints
{
    public static RouteGroupBuilder MapHostApartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/host/apartments")
            .WithTags("Host Apartments")
            .RequireAuthorization(ApplicationRoles.Host);

        group.MapPost("/", CreateApartment)
            .WithName("CreateHostApartment")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create apartment (Host)";
                operation.Description = "Creates an apartment owned by the authenticated Host. Returns 403 for Client role.";
                return operation;
            });

        group.MapGet("/", ListMyApartments)
            .WithName("ListHostApartments")
            .WithOpenApi(operation =>
            {
                operation.Summary = "List my apartments (Host)";
                operation.Description = "Returns apartments owned by the authenticated Host only.";
                return operation;
            });

        return group;
    }

    private static async Task<IResult> CreateApartment(
        CreateApartmentRequestDto request,
        ClaimsPrincipal user,
        IHostApartmentService hostApartmentService,
        CancellationToken cancellationToken)
    {
        var hostId = GetUserId(user);
        if (hostId is null)
            return Results.Unauthorized();

        var result = await hostApartmentService.CreateAsync(hostId, request, cancellationToken);
        return MapCreateResult(result);
    }

    private static async Task<IResult> ListMyApartments(
        ClaimsPrincipal user,
        IHostApartmentService hostApartmentService,
        CancellationToken cancellationToken)
    {
        var hostId = GetUserId(user);
        if (hostId is null)
            return Results.Unauthorized();

        var apartments = await hostApartmentService.ListMineAsync(hostId, cancellationToken);
        return Results.Ok(apartments);
    }

    private static IResult MapCreateResult(CreateApartmentResult result)
    {
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);

        return Results.Created($"/host/apartments/{result.Response!.Id}", result.Response);
    }

    private static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(ClaimTypes.Name);
}
