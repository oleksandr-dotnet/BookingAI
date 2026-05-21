using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public sealed class UserProfileResult
{
    public UserProfileDto? Response { get; init; }
    public bool NotFound { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static UserProfileResult Success(UserProfileDto response) =>
        new() { Response = response };

    public static UserProfileResult NotFoundResult() =>
        new() { NotFound = true };

    public static UserProfileResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { ValidationErrors = errors };
}

public sealed class UserDisplayResult
{
    public UserDisplayDto? Response { get; init; }
    public bool NotFound { get; init; }

    public static UserDisplayResult Success(UserDisplayDto response) =>
        new() { Response = response };

    public static UserDisplayResult NotFoundResult() =>
        new() { NotFound = true };
}
