using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BookingSystemAI.Api.OpenApi;

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT from POST /auth/login. Example: Authorization: Bearer {token}"
        };

        return Task.CompletedTask;
    }
}
