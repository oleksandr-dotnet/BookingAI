using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Services;

namespace BookingSystemAI.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a new user";
                operation.Description = "Creates an account with email and password.";
                return operation;
            });
        group.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Login";
                operation.Description = "Returns a JWT access token for authenticated requests.";
                return operation;
            });
        return group;
    }

    private static async Task<IResult> Register(
        RegisterRequestDto request,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.RegisterAsync(request, cancellationToken);
        return MapRegisterResult(result);
    }

    private static async Task<IResult> Login(
        LoginRequestDto request,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.LoginAsync(request, cancellationToken);
        return MapLoginResult(result);
    }

    private static IResult MapRegisterResult(RegisterResult result)
    {
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);

        return Results.Created($"/auth/users/{result.Response!.UserId}", result.Response);
    }

    private static IResult MapLoginResult(LoginResult result)
    {
        if (result.ValidationErrors is not null)
            return Results.ValidationProblem(result.ValidationErrors);

        if (!result.Succeeded)
            return Results.Unauthorized();

        return Results.Ok(result.Response);
    }
}
