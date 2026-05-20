using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public sealed class RegisterResult
{
    public bool Succeeded { get; init; }
    public RegisterResponseDto? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static RegisterResult Success(RegisterResponseDto response) =>
        new() { Succeeded = true, Response = response };

    public static RegisterResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Succeeded = false, ValidationErrors = errors };
}
