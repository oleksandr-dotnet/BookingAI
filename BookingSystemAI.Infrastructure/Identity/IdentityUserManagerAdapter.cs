using BookingSystemAI.Application.Abstractions;

using BookingSystemAI.Domain.Entities;

using Microsoft.AspNetCore.Identity;



namespace BookingSystemAI.Infrastructure.Identity;



public class IdentityUserManagerAdapter(

    UserManager<ApplicationUser> userManager,

    RoleManager<IdentityRole> roleManager) : IIdentityUserManager

{

    public async Task<IdentityCreateUserResult> CreateMigratedUserAsync(
        string email,
        string password,
        string role,
        Guid sourceCompanyId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var createResult = await CreateInternalAsync(
            email,
            password,
            sourceCompanyId,
            externalId,
            cancellationToken);

        if (!createResult.Succeeded || createResult.UserId is null)
            return createResult;

        var assignResult = await AssignRoleAsync(createResult.UserId, role, cancellationToken);
        return assignResult.Succeeded
            ? createResult
            : IdentityCreateUserResult.Failure(assignResult.Errors);
    }

    public async Task<IdentityCreateUserResult> CreateAsync(string email, string password,
        CancellationToken cancellationToken = default) =>
        await CreateInternalAsync(email, password, null, null, cancellationToken);

    private async Task<IdentityCreateUserResult> CreateInternalAsync(
        string email,
        string password,
        Guid? sourceCompanyId,
        string? externalId,
        CancellationToken cancellationToken)
    {
        var applicationUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            SourceCompanyId = sourceCompanyId,
            ExternalId = externalId
        };



        var result = await userManager.CreateAsync(applicationUser, password);

        if (!result.Succeeded)

        {

            var errors = result.Errors

                .GroupBy(e => e.Code.Contains("Password", StringComparison.OrdinalIgnoreCase) ? "password" : "email")

                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());



            return IdentityCreateUserResult.Failure(errors);

        }



        return IdentityCreateUserResult.Success(applicationUser.Id);

    }



    public async Task<IdentityAssignRoleResult> AssignRoleAsync(string userId, string role,

        CancellationToken cancellationToken = default)

    {

        if (!await roleManager.RoleExistsAsync(role))

        {

            return IdentityAssignRoleResult.Failure(new Dictionary<string, string[]>

            {

                ["role"] = [$"Role '{role}' does not exist."]

            });

        }



        var applicationUser = await userManager.FindByIdAsync(userId);

        if (applicationUser is null)

        {

            return IdentityAssignRoleResult.Failure(new Dictionary<string, string[]>

            {

                ["user"] = ["User not found."]

            });

        }



        var result = await userManager.AddToRoleAsync(applicationUser, role);

        if (!result.Succeeded)

        {

            var errors = result.Errors

                .GroupBy(e => "role")

                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return IdentityAssignRoleResult.Failure(errors);

        }



        return IdentityAssignRoleResult.Success();

    }



    public async Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default)

    {

        var applicationUser = await userManager.FindByIdAsync(userId);

        if (applicationUser is null)

            return [];



        var roles = await userManager.GetRolesAsync(applicationUser);

        return roles.ToList();

    }



    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)

    {

        var applicationUser = await userManager.FindByEmailAsync(email);

        return applicationUser is null ? null : MapToDomain(applicationUser);

    }



    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)

    {

        var applicationUser = await userManager.FindByIdAsync(user.Id);

        if (applicationUser is null)

            return false;



        return await userManager.CheckPasswordAsync(applicationUser, password);

    }



    private static User MapToDomain(ApplicationUser applicationUser) =>
        new()
        {
            Id = applicationUser.Id,
            Email = applicationUser.Email ?? string.Empty,
            SourceCompanyId = applicationUser.SourceCompanyId,
            ExternalId = applicationUser.ExternalId
        };

}

