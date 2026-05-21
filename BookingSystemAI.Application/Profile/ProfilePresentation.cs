using BookingSystemAI.Application;

namespace BookingSystemAI.Application.Profile;

public static class ProfilePresentation
{
    public static string BuildDisplayName(string? firstName, string? lastName, string email)
    {
        var first = firstName?.Trim();
        var last = lastName?.Trim();
        if (!string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(last))
            return $"{first} {last}";
        if (!string.IsNullOrEmpty(first))
            return first;
        if (!string.IsNullOrEmpty(last))
            return last;

        var at = email.IndexOf('@');
        return at > 0 ? email[..at] : email;
    }

    public static string BuildInitials(string? firstName, string? lastName, string email)
    {
        var first = firstName?.Trim();
        var last = lastName?.Trim();
        if (!string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(last))
            return $"{char.ToUpperInvariant(first[0])}{char.ToUpperInvariant(last[0])}";
        if (!string.IsNullOrEmpty(first))
            return char.ToUpperInvariant(first[0]).ToString();
        if (!string.IsNullOrEmpty(last))
            return char.ToUpperInvariant(last[0]).ToString();

        var local = email;
        var at = email.IndexOf('@');
        if (at > 0)
            local = email[..at];
        return string.IsNullOrEmpty(local) ? "?" : char.ToUpperInvariant(local[0]).ToString();
    }

    public static bool RequiresBirthDate(IReadOnlyList<string> roles) =>
        roles.Contains(ApplicationRoles.Client) || roles.Contains(ApplicationRoles.Host);

    public static bool IsProfileComplete(
        IReadOnlyList<string> roles,
        string? firstName,
        string? lastName,
        DateOnly? dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return false;

        return RequiresBirthDate(roles) ? dateOfBirth is not null : true;
    }
}
