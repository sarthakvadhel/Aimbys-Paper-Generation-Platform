namespace Aimbys.Web.Navigation;

/// <summary>
/// Static, in-memory catalogue of every role-area's navigation. Mirrors
/// the <c>NAV</c> arrays from the React shells one-for-one so any future
/// merge of the React tree picks up new sections without code changes
/// in the view layer:
///
/// <list type="bullet">
///   <item><c>SuperAdminShell.tsx</c> &mdash; sections Main / Management / Governance.</item>
///   <item><c>InstituteShell.tsx</c> &mdash; sections Overview / Academic / Workflow.</item>
///   <item><c>TeacherShell.tsx</c> &mdash; sections Overview / Authoring / Assessment.</item>
///   <item><c>StudentShell.tsx</c> &mdash; section Main.</item>
/// </list>
///
/// <para>
/// Only links whose underlying surface ships in this chunk are flagged
/// with <see cref="RoleNavLink.IsImplemented"/>; the rest render
/// disabled so reviewers can see the planned navigation surface
/// without dead links 404-ing.
/// </para>
/// </summary>
public static class RoleNavCatalog
{
    /// <summary>Brand-accent colour per area, copied from the React shells.</summary>
    public static string GetAccentHex(string area) => area switch
    {
        "SuperAdmin" => "#7c3aed", // violet
        "Institute"  => "#1d4ed8", // blue
        "Teacher"    => "#0369a1", // sky
        "Student"    => "#15803d", // green
        _            => "#1d4ed8"
    };

    /// <summary>Human-friendly label rendered in the sidebar header.</summary>
    public static string GetRoleLabel(string area) => area switch
    {
        "SuperAdmin" => "Super Admin",
        "Institute"  => "Institute",
        "Teacher"    => "Teacher",
        "Student"    => "Student",
        _            => area
    };

    /// <summary>
    /// Section list for the given area. Returns an empty list when the
    /// area name doesn't match a known shell &mdash; the layout copes
    /// gracefully (renders just the brand block + topbar).
    /// </summary>
    public static IReadOnlyList<RoleNavSection> ForArea(string area) => area switch
    {
        "SuperAdmin" => SuperAdminNav,
        "Institute"  => InstituteNav,
        "Teacher"    => TeacherNav,
        "Student"    => StudentNav,
        _            => Array.Empty<RoleNavSection>()
    };

    // The first link in every shell maps to a real Home/Index view
    // shipped in this chunk; downstream rows wait for their feature
    // chunk and render disabled.

    private static readonly IReadOnlyList<RoleNavSection> SuperAdminNav = new[]
    {
        new RoleNavSection("Main", new[]
        {
            new RoleNavLink("Dashboard",        "Index", "Home",        "SuperAdmin", "speedometer", IsImplemented: true),
            new RoleNavLink("Institutes",       "Index", "Institutes",  "SuperAdmin", "building", IsImplemented: true),
            new RoleNavLink("Global Analytics", "Index", "Analytics",   "SuperAdmin", "graph-up"),
        }),
        new RoleNavSection("Management", new[]
        {
            new RoleNavLink("Licenses",         "Index", "Licenses",    "SuperAdmin", "credit-card"),
            new RoleNavLink("Security Monitor", "Index", "Security",    "SuperAdmin", "shield"),
            new RoleNavLink("System Health",    "Index", "System",      "SuperAdmin", "hdd-stack"),
        }),
        new RoleNavSection("Governance", new[]
        {
            new RoleNavLink("Audit Logs",       "Index", "Audit",       "SuperAdmin", "activity"),
            new RoleNavLink("Broadcasts",       "Index", "Broadcasts",  "SuperAdmin", "broadcast"),
        }),
    };

    private static readonly IReadOnlyList<RoleNavSection> InstituteNav = new[]
    {
        new RoleNavSection("Overview", new[]
        {
            new RoleNavLink("Dashboard",        "Index", "Home",          "Institute", "speedometer", IsImplemented: true),
            new RoleNavLink("Users & Roles",    "Index", "Users",         "Institute", "people"),
        }),
        new RoleNavSection("Academic", new[]
        {
            new RoleNavLink("Paper Management", "Index", "Papers",        "Institute", "file-earmark-text"),
            new RoleNavLink("Question Bank",    "Index", "QuestionBank",  "Institute", "book"),
            new RoleNavLink("Exam Calendar",    "Index", "Calendar",      "Institute", "calendar"),
        }),
        new RoleNavSection("Workflow", new[]
        {
            new RoleNavLink("Approvals",        "Index", "Approvals",     "Institute", "check-square"),
            new RoleNavLink("Analytics",        "Index", "Analytics",     "Institute", "graph-up"),
            new RoleNavLink("Settings",         "Index", "Settings",      "Institute", "gear", IsImplemented: true),
        }),
    };

    private static readonly IReadOnlyList<RoleNavSection> TeacherNav = new[]
    {
        new RoleNavSection("Overview", new[]
        {
            new RoleNavLink("Dashboard",         "Index", "Home",          "Teacher", "speedometer", IsImplemented: true),
        }),
        new RoleNavSection("Authoring", new[]
        {
            new RoleNavLink("Paper Generation",  "Index", "PaperGen",      "Teacher", "file-earmark-text"),
            new RoleNavLink("Blueprints",        "Index", "Blueprints",    "Teacher", "book"),
            new RoleNavLink("Question Bank",     "Index", "QuestionBank",  "Teacher", "book"),
        }),
        new RoleNavSection("Assessment", new[]
        {
            new RoleNavLink("Evaluation Desk",   "Index", "Evaluation",    "Teacher", "pen", IsImplemented: true),
            new RoleNavLink("Reports",           "Index", "Reports",       "Teacher", "graph-up"),
            new RoleNavLink("Coding IDE",        "Index", "CodingIde",     "Teacher", "code"),
        }),
    };

    private static readonly IReadOnlyList<RoleNavSection> StudentNav = new[]
    {
        new RoleNavSection("Main", new[]
        {
            new RoleNavLink("Dashboard",     "Index", "Home",       "Student", "speedometer", IsImplemented: true),
            new RoleNavLink("My Exams",      "Index", "Exams",      "Student", "book"),
            new RoleNavLink("Results",       "Index", "Results",    "Student", "file-earmark-text"),
            new RoleNavLink("My Analytics",  "Index", "Analytics",  "Student", "graph-up"),
        }),
    };
}
