using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities;

/// <summary>
/// The tenancy root. Every other tenant-scoped entity (Department, Subject,
/// ClassBatch, TeacherProfile, StudentProfile, and the workflow entities
/// added in subsequent chunks) hangs off an <see cref="Institute"/> via
/// <c>InstituteId</c> so a single SQL Server database can host many
/// institutes with strict isolation enforced at the application layer.
/// </summary>
public class Institute
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Display name. Required, max 200 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short, human-friendly tenant code (e.g. <c>DPS-RKP</c>). Globally
    /// unique. Required, max 50 characters.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public InstituteType Type { get; set; } = InstituteType.School;

    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 or country name; default <c>"IN"</c>.</summary>
    public string Country { get; set; } = "IN";

    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }

    public InstituteStatus Status { get; set; } = InstituteStatus.PendingApproval;

    public LicenseTier LicenseTier { get; set; } = LicenseTier.Standard;

    /// <summary>Inclusive end of the current license window (UTC midnight).</summary>
    public DateTime? LicenseExpiresAtUtc { get; set; }

    /// <summary>Optional URL to the institute's logo for branding.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Optional brand primary colour in hex (e.g. <c>#0d1b2e</c>).</summary>
    public string? PrimaryColorHex { get; set; }

    /// <summary>
    /// Identity user id of the Super Admin who approved (or rejected) this
    /// institute. Null while <see cref="InstituteStatus.PendingApproval"/>.
    /// </summary>
    public string? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Soft-delete flag. Hard-delete is reserved for super-admin governance.</summary>
    public bool IsDeleted { get; set; }

    // Navigation
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<ClassBatch> ClassBatches { get; set; } = new List<ClassBatch>();
    public ICollection<TeacherProfile> Teachers { get; set; } = new List<TeacherProfile>();
    public ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
}
