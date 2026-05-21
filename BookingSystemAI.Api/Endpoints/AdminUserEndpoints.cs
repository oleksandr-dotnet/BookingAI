using BookingSystemAI.Application;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAI.Api.Endpoints;

public static class AdminUserEndpoints
{
    public static RouteGroupBuilder MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/users")
            .WithTags("Admin Users")
            .RequireAuthorization(ApplicationRoles.Admin);

        group.MapGet("/", ListUsers)
            .WithName("ListAdminUsers")
            .WithOpenApi(operation =>
            {
                operation.Summary = "List users (Admin)";
                operation.Description = "Paginated user list with optional role filter and email search.";
                return operation;
            });

        group.MapGet("/{userId}", GetUserById)
            .WithName("GetAdminUserById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get user profile (Admin)";
                operation.Description = "Returns user profile details for admin support.";
                return operation;
            });

        group.MapGet("/{userId}/bookings", ListUserBookings)
            .WithName("ListAdminUserBookings")
            .WithOpenApi(operation =>
            {
                operation.Summary = "List user bookings (Admin)";
                operation.Description = "Returns all bookings for the specified user.";
                return operation;
            });

        group.MapPost("/{userId}/lock", LockUser)
            .WithName("LockAdminUser")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lock user account (Admin)";
                return operation;
            });

        group.MapPost("/{userId}/unlock", UnlockUser)
            .WithName("UnlockAdminUser")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Unlock user account (Admin)";
                return operation;
            });

        group.MapPut("/{userId}/roles", SetUserRoles)
            .WithName("SetAdminUserRoles")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Set user roles (Admin)";
                operation.Description = "Replaces the user's role set. Cannot remove the last Admin.";
                return operation;
            });

        return group;
    }

    private static async Task<IResult> ListUsers(
        IAdminUserService adminUserService,
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var query = new AdminUserListQuery(
            role,
            search,
            page ?? 1,
            pageSize ?? 20,
            sort ?? "email");

        var result = await adminUserService.ListUsersAsync(query, cancellationToken);
        return MapListResult(result);
    }

    private static async Task<IResult> GetUserById(
        string userId,
        IAdminUserService adminUserService,
        CancellationToken cancellationToken)
    {
        var result = await adminUserService.GetUserByIdAsync(userId, cancellationToken);
        if (result.NotFound)
            return Results.NotFound();

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> ListUserBookings(
        string userId,
        IAdminUserService adminUserService,
        CancellationToken cancellationToken)
    {
        var userResult = await adminUserService.GetUserByIdAsync(userId, cancellationToken);
        if (userResult.NotFound)
            return Results.NotFound();

        var bookings = await adminUserService.ListUserBookingsAsync(userId, cancellationToken);
        return Results.Ok(bookings);
    }

    private static async Task<IResult> LockUser(
        string userId,
        IAdminUserService adminUserService,
        CancellationToken cancellationToken)
    {
        var result = await adminUserService.LockUserAsync(userId, cancellationToken);
        return MapActionResult(result);
    }

    private static async Task<IResult> UnlockUser(
        string userId,
        IAdminUserService adminUserService,
        CancellationToken cancellationToken)
    {
        var result = await adminUserService.UnlockUserAsync(userId, cancellationToken);
        return MapActionResult(result);
    }

    private static async Task<IResult> SetUserRoles(
        string userId,
        SetUserRolesRequestDto request,
        IAdminUserService adminUserService,
        CancellationToken cancellationToken)
    {
        var result = await adminUserService.SetRolesAsync(userId, request, cancellationToken);
        return MapRolesResult(result);
    }

    private static IResult MapListResult(AdminUserListResult result)
    {
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);

        var paged = result.Response!;
        return Results.Ok(new AdminUserListResponseDto(
            paged.Items,
            paged.Page,
            paged.PageSize,
            paged.TotalCount));
    }

    private static IResult MapActionResult(AdminUserActionResult result)
    {
        if (result.NotFound)
            return Results.NotFound();
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);
        return Results.NoContent();
    }

    private static IResult MapRolesResult(AdminUserActionResult result)
    {
        if (result.NotFound)
            return Results.NotFound();
        if (result.Conflict)
            return Results.Conflict(new { message = result.ConflictMessage });
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);
        return Results.Ok(result.Response);
    }
}
