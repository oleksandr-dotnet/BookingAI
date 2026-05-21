using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;
using BookingSystemAI.Application.Profile;

namespace BookingSystemAI.Application.Services;

public class UserProfileService(IUserProfileStore profileStore, IImageUrlValidator imageUrlValidator) : IUserProfileService
{
    private const int MaxNameLength = 100;
    private const int MinAgeYears = 13;

    public async Task<UserProfileResult> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return UserProfileResult.NotFoundResult();

        var record = await profileStore.GetByUserIdAsync(userId, cancellationToken);
        return record is null
            ? UserProfileResult.NotFoundResult()
            : UserProfileResult.Success(MapProfile(record));
    }

    public async Task<UserProfileResult> UpdateMyProfileAsync(
        string userId,
        UpdateUserProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return UserProfileResult.NotFoundResult();

        var existing = await profileStore.GetByUserIdAsync(userId, cancellationToken);
        if (existing is null)
            return UserProfileResult.NotFoundResult();

        var validationErrors = ValidateUpdate(existing.Roles, request);
        if (validationErrors is not null)
            return UserProfileResult.ValidationFailure(validationErrors);

        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();
        var profileImageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl)
            ? null
            : request.ProfileImageUrl.Trim();

        var updated = await profileStore.UpdateProfileAsync(
            userId,
            firstName,
            lastName,
            request.DateOfBirth,
            profileImageUrl,
            cancellationToken);

        if (!updated)
            return UserProfileResult.NotFoundResult();

        var record = await profileStore.GetByUserIdAsync(userId, cancellationToken);
        return record is null
            ? UserProfileResult.NotFoundResult()
            : UserProfileResult.Success(MapProfile(record));
    }

    public async Task<UserDisplayResult> GetUserDisplayAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return UserDisplayResult.NotFoundResult();

        var record = await profileStore.GetByUserIdAsync(userId, cancellationToken);
        if (record is null)
            return UserDisplayResult.NotFoundResult();

        return UserDisplayResult.Success(new UserDisplayDto(
            record.UserId,
            ProfilePresentation.BuildDisplayName(record.FirstName, record.LastName, record.Email),
            record.ProfileImageUrl,
            ProfilePresentation.BuildInitials(record.FirstName, record.LastName, record.Email)));
    }

    private static UserProfileDto MapProfile(UserProfileRecord record) =>
        new(
            record.UserId,
            record.Email,
            record.UserName,
            record.Roles,
            record.FirstName,
            record.LastName,
            record.DateOfBirth,
            record.ProfileImageUrl,
            ProfilePresentation.BuildDisplayName(record.FirstName, record.LastName, record.Email),
            ProfilePresentation.BuildInitials(record.FirstName, record.LastName, record.Email),
            ProfilePresentation.IsProfileComplete(
                record.Roles,
                record.FirstName,
                record.LastName,
                record.DateOfBirth));

    private IReadOnlyDictionary<string, string[]>? ValidateUpdate(
        IReadOnlyList<string> roles,
        UpdateUserProfileRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors["firstName"] = ["First name is required."];
        else if (request.FirstName.Trim().Length > MaxNameLength)
            errors["firstName"] = [$"First name must be at most {MaxNameLength} characters."];

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors["lastName"] = ["Last name is required."];
        else if (request.LastName.Trim().Length > MaxNameLength)
            errors["lastName"] = [$"Last name must be at most {MaxNameLength} characters."];

        if (ProfilePresentation.RequiresBirthDate(roles))
        {
            if (request.DateOfBirth is null)
                errors["dateOfBirth"] = ["Date of birth is required."];
            else
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (request.DateOfBirth > today)
                    errors["dateOfBirth"] = ["Date of birth cannot be in the future."];
                else
                {
                    var age = today.Year - request.DateOfBirth.Value.Year;
                    if (request.DateOfBirth.Value > today.AddYears(-age))
                        age--;
                    if (age < MinAgeYears)
                        errors["dateOfBirth"] = [$"You must be at least {MinAgeYears} years old."];
                }
            }
        }
        else if (request.DateOfBirth is not null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.DateOfBirth > today)
                errors["dateOfBirth"] = ["Date of birth cannot be in the future."];
        }

        if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl))
        {
            var urlErrors = imageUrlValidator.ValidateUrls([request.ProfileImageUrl.Trim()]);
            if (urlErrors is not null)
            {
                foreach (var pair in urlErrors)
                    errors["profileImageUrl"] = pair.Value;
            }
        }

        return errors.Count > 0 ? errors : null;
    }
}
