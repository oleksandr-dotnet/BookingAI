using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Services;
namespace BookingSystemAI.Api.Endpoints;

public static class ApartmentEndpoints
{
    public static RouteGroupBuilder MapApartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/apartments").WithTags("Apartments");
        group.MapGet("/", ListApartments)
            .WithName("ListApartments")
            .WithOpenApi(operation =>
            {
                operation.Summary = "List apartments";
                operation.Description =
                    "Public catalog. Optional from/to (ISO 8601) add isAvailable; availableOnly=true filters to free apartments.";
                return operation;
            });
        return group;
    }

    private static async Task<IResult> ListApartments(
        IApartmentService apartmentService,
        CancellationToken cancellationToken,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        bool? availableOnly = null)
    {
        try
        {
            var query = new ListApartmentsQueryDto(from, to, availableOnly ?? false);
            var apartments = await apartmentService.ListCatalogAsync(query, cancellationToken);
            return Results.Ok(apartments);
        }
        catch (ApartmentQueryValidationException ex)
        {
            return Results.ValidationProblem(ex.Errors);
        }
    }
}
