using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public interface IAdminUserService
{
    Task<AdminUserListResult> ListUsersAsync(AdminUserListQuery query, CancellationToken cancellationToken = default);
    Task<AdminUserDetailResult> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingResponseDto>> ListUserBookingsAsync(string userId, CancellationToken cancellationToken = default);
    Task<AdminUserActionResult> LockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<AdminUserActionResult> UnlockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<AdminUserActionResult> SetRolesAsync(string userId, SetUserRolesRequestDto request, CancellationToken cancellationToken = default);
}
