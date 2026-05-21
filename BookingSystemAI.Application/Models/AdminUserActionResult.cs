using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public sealed class AdminUserActionResult
{
    public bool NotFound { get; init; }
    public bool Conflict { get; init; }
    public string? ConflictMessage { get; init; }
    public AdminUserDetailDto? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static AdminUserActionResult Success(AdminUserDetailDto? response = null) =>
        new() { Response = response };

    public static AdminUserActionResult NotFoundResult() => new() { NotFound = true };

    public static AdminUserActionResult ConflictResult(string message) =>
        new() { Conflict = true, ConflictMessage = message };

    public static AdminUserActionResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { ValidationErrors = errors };
}
