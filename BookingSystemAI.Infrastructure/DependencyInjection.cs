using System.Text;
using BookingSystemAI.Application;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using BookingSystemAI.Infrastructure.Identity;
using BookingSystemAI.Infrastructure.Options;
using BookingSystemAI.Infrastructure.CompanyImport;
using BookingSystemAI.Infrastructure.Repositories;
using BookingSystemAI.Infrastructure.Services;
using BookingSystemAI.Infrastructure.Sql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BookingSystemAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is required.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key))
            throw new InvalidOperationException("Jwt:Key must be configured.");

        var connectionString = ConnectionStringNormalizer.Normalize(
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured."));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.ConfigureDataSource(builder => builder.EnableDynamicJson())));

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(ApplicationRoles.Host, policy => policy.RequireRole(ApplicationRoles.Host))
            .AddPolicy(ApplicationRoles.Client, policy => policy.RequireRole(ApplicationRoles.Client))
            .AddPolicy(ApplicationRoles.Admin, policy => policy.RequireRole(ApplicationRoles.Admin));
        services.AddScoped<IIdentityUserManager, IdentityUserManagerAdapter>();
        services.AddScoped<IAdminUserQuery, IdentityAdminUserQuery>();
        services.AddScoped<IUserProfileStore, IdentityUserProfileStore>();
        services.AddScoped<IAdminIdentityOperations, IdentityAdminOperationsAdapter>();
        services.AddScoped<IAdminBookingQuery, EfAdminBookingQuery>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IApartmentRepository, ApartmentRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddSingleton<ISqlScriptLoader, SqlScriptLoader>();
        services.AddScoped<INpgsqlConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IApartmentSqlGateway, DapperApartmentSqlGateway>();
        services.AddScoped<IJsonExportReader, JsonExportReader>();
        services.AddScoped<IMigrationTransactor, MigrationTransactor>();
        services.AddScoped<IExternalEntityLookup, ExternalEntityLookup>();
        services.AddScoped<IMigratedApartmentWriter, MigratedApartmentWriter>();
        services.AddSingleton<IImageUrlValidator, CloudinaryImageUrlValidator>();
        services.AddSingleton<IImageUploadConfigService, CloudinaryImageUploadConfigService>();

        var stripeOptions = configuration.GetSection(StripeOptions.SectionName).Get<StripeOptions>() ?? new StripeOptions();
        var useFakeStripe = stripeOptions.UseFakeGateway
            || environment?.IsEnvironment("Testing") == true
            || string.IsNullOrWhiteSpace(stripeOptions.SecretKey);

        if (useFakeStripe)
            services.AddSingleton<IStripeCheckoutGateway, FakeStripeCheckoutGateway>();
        else
            services.AddSingleton<IStripeCheckoutGateway, StripeCheckoutGateway>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await db.Database.MigrateAsync();
        await IdentityRoleSeeder.SeedAsync(services);
        await IdentityAdminSeeder.SeedAsync(services, configuration);
    }

    public static async Task SeedDevDataAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
            return;

        if (!configuration.GetValue("SeedDevData", defaultValue: false))
            return;

        await DevDataSeeder.SeedAsync(services, cancellationToken);
    }
}
