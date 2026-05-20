using BookingSystemAI.Application;
using BookingSystemAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystemAI.Infrastructure.Identity;

public static class IdentityAdminSeeder
{
    public const string DefaultEmail = "admin@bookingsystem.local";
    public const string DefaultPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["Admin:Email"] ?? DefaultEmail;
        var password = configuration["Admin:Password"] ?? DefaultPassword;

        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(ApplicationRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Admin));

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
            await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);
    }
}
