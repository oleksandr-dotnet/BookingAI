using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Abstractions;

public interface IUserProfileStore
{
    Task<UserProfileRecord?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(
        string userId,
        string? firstName,
        string? lastName,
        DateOnly? dateOfBirth,
        string? profileImageUrl,
        CancellationToken cancellationToken = default);
}
