using BookingSystemAI.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.Identity;

public class IdentityAdminOperationsAdapter(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    : IAdminIdentityOperations
{
    private static readonly DateTimeOffset FarFutureLockout = DateTimeOffset.UtcNow.AddYears(100);

    public Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken = default) =>
        userManager.Users.AsNoTracking().AnyAsync(u => u.Id == userId, cancellationToken);

    public async Task<int> CountUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var usersInRole = await userManager.GetUsersInRoleAsync(role);
        return usersInRole.Count;
    }

    public async Task<AdminIdentityOperationResult> LockUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return AdminIdentityOperationResult.NotFoundResult();

        user.LockoutEnabled = true;
        user.LockoutEnd = FarFutureLockout;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? AdminIdentityOperationResult.Success()
            : AdminIdentityOperationResult.Failure(ToErrors(result));
    }

    public async Task<AdminIdentityOperationResult> UnlockUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return AdminIdentityOperationResult.NotFoundResult();

        user.LockoutEnd = null;
        user.LockoutEnabled = false;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? AdminIdentityOperationResult.Success()
            : AdminIdentityOperationResult.Failure(ToErrors(result));
    }

    public async Task<AdminIdentityOperationResult> SetRolesAsync(
        string userId,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return AdminIdentityOperationResult.NotFoundResult();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                return AdminIdentityOperationResult.Failure(new Dictionary<string, string[]>
                {
                    ["roles"] = [$"Role '{role}' does not exist."]
                });
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var toRemove = currentRoles.Where(r => !roles.Contains(r, StringComparer.Ordinal)).ToList();
        var toAdd = roles.Where(r => !currentRoles.Contains(r, StringComparer.Ordinal)).ToList();

        if (toRemove.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, toRemove);
            if (!removeResult.Succeeded)
                return AdminIdentityOperationResult.Failure(ToErrors(removeResult));
        }

        if (toAdd.Count > 0)
        {
            var addResult = await userManager.AddToRolesAsync(user, toAdd);
            if (!addResult.Succeeded)
                return AdminIdentityOperationResult.Failure(ToErrors(addResult));
        }

        return AdminIdentityOperationResult.Success();
    }

    private static IReadOnlyDictionary<string, string[]> ToErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}
