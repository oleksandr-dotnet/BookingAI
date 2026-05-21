namespace BookingSystemAI.Application.Abstractions;

public interface IAdminIdentityOperations
{
    Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> CountUsersInRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<AdminIdentityOperationResult> LockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<AdminIdentityOperationResult> UnlockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<AdminIdentityOperationResult> SetRolesAsync(
        string userId,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken = default);
}

public sealed class AdminIdentityOperationResult
{
    public bool Succeeded { get; init; }
    public bool NotFound { get; init; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    public static AdminIdentityOperationResult Success() => new() { Succeeded = true };

    public static AdminIdentityOperationResult NotFoundResult() => new() { NotFound = true };

    public static AdminIdentityOperationResult Failure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Errors = errors };
}
