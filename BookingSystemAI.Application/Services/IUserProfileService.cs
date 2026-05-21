using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public interface IUserProfileService
{
    Task<UserProfileResult> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserProfileResult> UpdateMyProfileAsync(
        string userId,
        UpdateUserProfileRequestDto request,
        CancellationToken cancellationToken = default);
    Task<UserDisplayResult> GetUserDisplayAsync(string userId, CancellationToken cancellationToken = default);
}
