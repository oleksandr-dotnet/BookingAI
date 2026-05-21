using BookingSystemAI.Application;
using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Listing;
using BookingSystemAI.Application.Profile;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public class AdminUserService(
    IAdminUserQuery adminUserQuery,
    IBookingRepository bookingRepository,
    IAdminIdentityOperations adminIdentityOperations) : IAdminUserService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<AdminUserListResult> ListUsersAsync(
        AdminUserListQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateListQuery(query);
        if (validationErrors is not null)
            return AdminUserListResult.ValidationFailure(validationErrors);

        var normalized = NormalizeQuery(query);
        var (items, totalCount) = await adminUserQuery.ListAsync(normalized, cancellationToken);

        var dtos = items.Select(MapListItem).ToList();
        return AdminUserListResult.Success(new PagedResult<AdminUserListItemDto>
        {
            Items = dtos,
            Page = normalized.Page,
            PageSize = normalized.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<AdminUserDetailResult> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return AdminUserDetailResult.NotFoundResult();

        var record = await adminUserQuery.GetByIdAsync(userId, cancellationToken);
        return record is null
            ? AdminUserDetailResult.NotFoundResult()
            : AdminUserDetailResult.Success(MapDetail(record));
    }

    public async Task<IReadOnlyList<BookingResponseDto>> ListUserBookingsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await bookingRepository.ListByUserIdWithApartmentAsync(userId, cancellationToken);
        return rows.Select(BookingDtoMapper.ToListResponse).ToList();
    }

    public async Task<AdminUserActionResult> LockUserAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        await ExecuteIdentityActionAsync(userId, () => adminIdentityOperations.LockUserAsync(userId, cancellationToken), cancellationToken);

    public async Task<AdminUserActionResult> UnlockUserAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        await ExecuteIdentityActionAsync(userId, () => adminIdentityOperations.UnlockUserAsync(userId, cancellationToken), cancellationToken);

    public async Task<AdminUserActionResult> SetRolesAsync(
        string userId,
        SetUserRolesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return AdminUserActionResult.NotFoundResult();

        var validationErrors = ValidateRoles(request.Roles);
        if (validationErrors is not null)
            return AdminUserActionResult.ValidationFailure(validationErrors);

        if (!await adminIdentityOperations.UserExistsAsync(userId, cancellationToken))
            return AdminUserActionResult.NotFoundResult();

        var roles = request.Roles.Distinct(StringComparer.Ordinal).ToList();
        if (!roles.Contains(ApplicationRoles.Admin))
        {
            var adminCount = await adminIdentityOperations.CountUsersInRoleAsync(ApplicationRoles.Admin, cancellationToken);
            if (adminCount <= 1)
            {
                var record = await adminUserQuery.GetByIdAsync(userId, cancellationToken);
                if (record?.Roles.Contains(ApplicationRoles.Admin) == true)
                    return AdminUserActionResult.ConflictResult("Cannot remove the last Admin account.");
            }
        }

        var opResult = await adminIdentityOperations.SetRolesAsync(userId, roles, cancellationToken);
        if (opResult.NotFound)
            return AdminUserActionResult.NotFoundResult();
        if (opResult.Errors is not null)
            return AdminUserActionResult.ValidationFailure(opResult.Errors);

        var updated = await adminUserQuery.GetByIdAsync(userId, cancellationToken);
        return updated is null
            ? AdminUserActionResult.NotFoundResult()
            : AdminUserActionResult.Success(MapDetail(updated));
    }

    private async Task<AdminUserActionResult> ExecuteIdentityActionAsync(
        string userId,
        Func<Task<AdminIdentityOperationResult>> action,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return AdminUserActionResult.NotFoundResult();

        if (!await adminIdentityOperations.UserExistsAsync(userId, cancellationToken))
            return AdminUserActionResult.NotFoundResult();

        var opResult = await action();
        if (opResult.NotFound)
            return AdminUserActionResult.NotFoundResult();
        if (opResult.Errors is not null)
            return AdminUserActionResult.ValidationFailure(opResult.Errors);

        return AdminUserActionResult.Success();
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateRoles(IReadOnlyList<string> roles)
    {
        var errors = new Dictionary<string, string[]>();
        if (roles is null || roles.Count == 0)
            errors["roles"] = ["At least one role is required."];
        else if (roles.Any(r => !ApplicationRoles.All.Contains(r)))
            errors["roles"] = [$"Each role must be one of: {string.Join(", ", ApplicationRoles.All)}."];
        return errors.Count > 0 ? errors : null;
    }

    private static IReadOnlyDictionary<string, string[]>? ValidateListQuery(AdminUserListQuery query)
    {
        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(query.Role) && !ApplicationRoles.All.Contains(query.Role))
            errors["role"] = [$"Role must be one of: {string.Join(", ", ApplicationRoles.All)}."];

        if (query.Page < 1)
            errors["page"] = ["Page must be at least 1."];

        if (query.PageSize > MaxPageSize)
            errors["pageSize"] = [$"PageSize must be at most {MaxPageSize}."];

        var sort = query.Sort.Trim().ToLowerInvariant();
        if (sort is not "email" and not "created")
            errors["sort"] = ["Sort must be 'email' or 'created'."];

        return errors.Count > 0 ? errors : null;
    }

    private static AdminUserListQuery NormalizeQuery(AdminUserListQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize
        };

        return query with
        {
            Page = page,
            PageSize = pageSize,
            Sort = query.Sort.Trim().ToLowerInvariant(),
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            Role = string.IsNullOrWhiteSpace(query.Role) ? null : query.Role
        };
    }

    private static AdminUserListItemDto MapListItem(AdminUserRecord record) =>
        new(
            record.UserId,
            record.Email,
            record.UserName,
            record.Roles,
            record.EmailConfirmed,
            record.LockoutEnabled,
            record.LockoutEnd,
            record.BookingCount,
            record.FirstName,
            record.LastName,
            record.DateOfBirth,
            record.ProfileImageUrl,
            ProfilePresentation.BuildDisplayName(record.FirstName, record.LastName, record.Email));

    private static AdminUserDetailDto MapDetail(AdminUserRecord record) =>
        new(
            record.UserId,
            record.Email,
            record.UserName,
            record.Roles,
            record.EmailConfirmed,
            record.LockoutEnabled,
            record.LockoutEnd,
            record.SourceCompanyId,
            record.ExternalId,
            record.FirstName,
            record.LastName,
            record.DateOfBirth,
            record.ProfileImageUrl,
            ProfilePresentation.BuildDisplayName(record.FirstName, record.LastName, record.Email));
}
