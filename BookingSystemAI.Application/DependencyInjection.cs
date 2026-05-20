using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystemAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IApartmentService, ApartmentService>();
        services.AddScoped<IHostApartmentService, HostApartmentService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IApartmentUpsertService, ApartmentUpsertService>();
        services.AddScoped<IApartmentAnalyticsService, ApartmentAnalyticsService>();
        services.AddScoped<ICompanyMigrationService, CompanyMigrationService>();
        return services;
    }
}
