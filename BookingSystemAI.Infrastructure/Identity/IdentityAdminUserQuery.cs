using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Profile;
using BookingSystemAI.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.Identity;

public class IdentityAdminUserQuery(ApplicationDbContext dbContext) : IAdminUserQuery
{
    public async Task<(IReadOnlyList<AdminUserRecord> Items, int TotalCount)> ListAsync(
        AdminUserListQuery query,
        CancellationToken cancellationToken = default)
    {
        var usersQuery = dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search}%";
            usersQuery = usersQuery.Where(u => u.Email != null && EF.Functions.ILike(u.Email, pattern));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            usersQuery = usersQuery.Where(u =>
                dbContext.UserRoles.Any(ur =>
                    ur.UserId == u.Id &&
                    dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == query.Role)));
        }

        usersQuery = query.Sort == "created"
            ? usersQuery.OrderBy(u => u.Id)
            : usersQuery.OrderBy(u => u.Email);

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var skip = (query.Page - 1) * query.PageSize;
        var users = await usersQuery
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var records = await MapUsersWithRolesAsync(users, cancellationToken);
        return (records, totalCount);
    }

    public async Task<AdminUserRecord?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;

        var records = await MapUsersWithRolesAsync([user], cancellationToken);
        return records.FirstOrDefault();
    }

    private async Task<IReadOnlyList<AdminUserRecord>> MapUsersWithRolesAsync(
        IReadOnlyList<ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        if (users.Count == 0)
            return [];

        var userIds = users.Select(u => u.Id).ToList();
        var roleRows = await (
            from ur in dbContext.UserRoles.AsNoTracking()
            join r in dbContext.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId) && r.Name != null
            select new { ur.UserId, RoleName = r.Name! }
        ).ToListAsync(cancellationToken);

        var rolesByUser = roleRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.RoleName).OrderBy(n => n).ToList());

        var bookingCounts = await dbContext.Bookings.AsNoTracking()
            .Where(b => userIds.Contains(b.UserId))
            .GroupBy(b => b.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

        return users.Select(u => new AdminUserRecord
        {
            UserId = u.Id,
            Email = u.Email ?? string.Empty,
            UserName = u.UserName,
            Roles = rolesByUser.TryGetValue(u.Id, out var roles) ? roles : [],
            EmailConfirmed = u.EmailConfirmed,
            LockoutEnabled = u.LockoutEnabled,
            LockoutEnd = u.LockoutEnd,
            SourceCompanyId = u.SourceCompanyId,
            ExternalId = u.ExternalId,
            BookingCount = bookingCounts.GetValueOrDefault(u.Id),
            FirstName = u.FirstName,
            LastName = u.LastName,
            DateOfBirth = u.DateOfBirth,
            ProfileImageUrl = u.ProfileImageUrl
        }).ToList();
    }
}
