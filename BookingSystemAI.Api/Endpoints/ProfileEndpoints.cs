using System.Security.Claims;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAI.Api.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/profile")
            .WithTags("Profile")
            .RequireAuthorization();

        group.MapGet("/me", GetMyProfile)
            .WithName("GetMyProfile")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get current user profile";
                operation.Description = "Returns the authenticated user's profile including completeness flag.";
                return operation;
            });

        group.MapPut("/me", UpdateMyProfile)
            .WithName("UpdateMyProfile")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update current user profile";
                operation.Description = "Updates profile fields with role-based validation.";
                return operation;
            });

        var users = app.MapGroup("/users")
            .WithTags("Profile")
            .RequireAuthorization();

        users.MapGet("/{userId}/display", GetUserDisplay)
            .WithName("GetUserDisplay")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get user display card";
                operation.Description = "Returns display name, avatar URL, and initials for referencing a user.";
                return operation;
            });

        return group;
    }

    private static async Task<IResult> GetMyProfile(
        ClaimsPrincipal user,
        IUserProfileService profileService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Results.Unauthorized();

        var result = await profileService.GetMyProfileAsync(userId, cancellationToken);
        if (result.NotFound)
            return Results.NotFound();

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> UpdateMyProfile(
        ClaimsPrincipal user,
        [FromBody] UpdateUserProfileRequestDto request,
        IUserProfileService profileService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return Results.Unauthorized();

        var result = await profileService.UpdateMyProfileAsync(userId, request, cancellationToken);
        if (result.NotFound)
            return Results.NotFound();
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> GetUserDisplay(
        string userId,
        IUserProfileService profileService,
        CancellationToken cancellationToken)
    {
        var result = await profileService.GetUserDisplayAsync(userId, cancellationToken);
        return result.NotFound ? Results.NotFound() : Results.Ok(result.Response);
    }

    private static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);
}
