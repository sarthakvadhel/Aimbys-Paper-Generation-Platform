using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities;

/// <summary>
/// Profile row for a teacher / examiner / evaluator / moderator inside an
/// <see cref="Institute"/>. The login credentials live on
/// <c>AspNetUsers</c> (referenced by <see cref="UserId"/>) &mdash; this row
/// holds the institutional metadata and the operational permission flags
/// the React reference exposes via the <c>InstituteUsers</c> screen.
/// </summary>
public class TeacherProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK to <c>AspNetUsers.Id</c> (the Identity user). Unique.</summary>
    public string UserId { get; set; } = string.Empty;

    public Guid InstituteId { get; set; }
    public Guid? DepartmentId { get; set; }

    /// <summary>Cached display name, max 200.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Optional employee code, unique within the institute.</summary>
    public string? EmployeeCode { get; set; }

    /// <summary>Free-form designation (e.g. <c>"Senior Teacher"</c>, <c>"HoD"</c>), max 100.</summary>
    public string? Designation { get; set; }

    public ProfileStatus Status { get; set; } = ProfileStatus.Active;

    // Operational permission flags &mdash; set per-teacher by the institute admin.
    public bool CanAuthorQuestions { get; set; } = true;
    public bool CanGeneratePapers { get; set; }
    public bool CanEvaluate { get; set; }
    public bool CanModerate { get; set; }
    public bool CanReview { get; set; }
    public bool CanProctor { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Institute? Institute { get; set; }
    public Department? Department { get; set; }
}
