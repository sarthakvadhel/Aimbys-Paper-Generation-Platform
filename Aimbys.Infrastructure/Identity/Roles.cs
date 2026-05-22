namespace Aimbys.Infrastructure.Identity;

/// <summary>
/// Canonical role names used by the application. Use these constants
/// everywhere instead of magic strings so a typo is a compile error.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";

    /// <summary>The full set of roles seeded at application startup.</summary>
    public static readonly IReadOnlyList<string> All = new[] { Admin, User };
}
