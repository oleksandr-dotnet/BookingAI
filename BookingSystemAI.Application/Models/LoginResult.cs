using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public sealed class LoginResult
{
    public bool Succeeded { get; init; }
    public AuthResponseDto? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static LoginResult Success(AuthResponseDto response) =>
        new() { Succeeded = true, Response = response };

    public static LoginResult Unauthorized() =>
        new() { Succeeded = false };

    public static LoginResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Succeeded = false, ValidationErrors = errors };
}
