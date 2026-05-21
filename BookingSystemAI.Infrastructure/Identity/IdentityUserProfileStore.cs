using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.Identity;

public class IdentityUserProfileStore(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : IUserProfileStore
{
    public async Task<UserProfileRecord?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;

        var roles = await GetRolesAsync(userId, cancellationToken);
        return Map(user, roles);
    }

    public async Task<bool> UpdateProfileAsync(
        string userId,
        string? firstName,
        string? lastName,
        DateOnly? dateOfBirth,
        string? profileImageUrl,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        user.FirstName = firstName;
        user.LastName = lastName;
        user.DateOfBirth = dateOfBirth;
        user.ProfileImageUrl = profileImageUrl;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    private async Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken)
    {
        var roleRows = await (
            from ur in dbContext.UserRoles.AsNoTracking()
            join r in dbContext.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId && r.Name != null
            select r.Name!
        ).ToListAsync(cancellationToken);

        return roleRows.OrderBy(n => n).ToList();
    }

    private static UserProfileRecord Map(ApplicationUser user, IReadOnlyList<string> roles) =>
        new()
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName,
            Roles = roles,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            ProfileImageUrl = user.ProfileImageUrl
        };
}
