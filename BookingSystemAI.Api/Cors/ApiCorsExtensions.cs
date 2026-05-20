namespace BookingSystemAI.Api.Cors;

public static class ApiCorsExtensions
{
    public const string DevelopmentPolicy = "Development";
    public const string DeployedPolicy = "Deployed";

    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentPolicy, policy =>
                policy.WithOrigins(
                        "http://localhost:5173",
                        "http://127.0.0.1:5173",
                        "http://[::1]:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            var allowedOrigins = GetAllowedOrigins(configuration);
            if (allowedOrigins.Length > 0)
            {
                options.AddPolicy(DeployedPolicy, policy =>
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            }
        });

        return services;
    }

    public static string? ResolveCorsPolicyName(IHostEnvironment environment, IConfiguration configuration)
    {
        if (environment.IsDevelopment())
            return DevelopmentPolicy;

        if (environment.IsEnvironment("Testing"))
            return null;

        return GetAllowedOrigins(configuration).Length > 0 ? DeployedPolicy : null;
    }

    public static void ValidateHostedCors(IHostEnvironment environment, IConfiguration configuration)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
            return;

        if (GetAllowedOrigins(configuration).Length == 0)
        {
            throw new InvalidOperationException(
                "CORS is not configured for hosted deployment. Set environment variable " +
                "Cors__AllowedOrigins or TEST_UI_URL to the UI origin " +
                "(example: https://bookingai-9702.onrender.com, no trailing slash).");
        }
    }

    public static void UseApiCors(this WebApplication app, string? policyName)
    {
        if (policyName is null)
            return;

        app.UseRouting();
        app.UseCors(policyName);
    }

    public static RouteGroupBuilder WithApiCors(this RouteGroupBuilder group, string? policyName)
    {
        if (policyName is not null)
            group.RequireCors(policyName);

        return group;
    }

    public static IEndpointConventionBuilder WithApiCors(this IEndpointConventionBuilder builder, string? policyName)
    {
        if (policyName is not null)
            builder.RequireCors(policyName);

        return builder;
    }

    public static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        var raw = configuration["Cors:AllowedOrigins"]
                  ?? configuration["TEST_UI_URL"];

        if (string.IsNullOrWhiteSpace(raw))
            return [];

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(o => o.TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
