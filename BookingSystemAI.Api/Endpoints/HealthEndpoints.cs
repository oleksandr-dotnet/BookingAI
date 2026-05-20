namespace BookingSystemAI.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("Health")
            .AllowAnonymous()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Health check";
                operation.Description = "Returns 200 when the API process is running. Used by hosting platforms.";
                return operation;
            });
        return app;
    }
}
