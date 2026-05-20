using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Abstractions;

public interface IIdentityUserManager
{
    Task<IdentityCreateUserResult> CreateAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<IdentityAssignRoleResult> AssignRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<IdentityCreateUserResult> CreateMigratedUserAsync(
        string email,
        string password,
        string role,
        Guid sourceCompanyId,
        string externalId,
        CancellationToken cancellationToken = default);
}

public sealed class IdentityAssignRoleResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();

    public static IdentityAssignRoleResult Success() => new() { Succeeded = true };

    public static IdentityAssignRoleResult Failure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Succeeded = false, Errors = errors };
}

public sealed class IdentityCreateUserResult
{
    public bool Succeeded { get; init; }
    public string? UserId { get; init; }
    public IReadOnlyDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();

    public static IdentityCreateUserResult Success(string userId) =>
        new() { Succeeded = true, UserId = userId };

    public static IdentityCreateUserResult Failure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Succeeded = false, Errors = errors };
}
