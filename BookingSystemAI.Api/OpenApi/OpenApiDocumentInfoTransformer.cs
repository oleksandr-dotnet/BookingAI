using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BookingSystemAI.Api.OpenApi;

internal sealed class OpenApiDocumentInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Booking System API",
            Version = "v1",
            Description = "Booking system API with JWT bearer authentication."
        };

        return Task.CompletedTask;
    }
}
