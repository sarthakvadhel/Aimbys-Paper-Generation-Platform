namespace Aimbys.Domain.Entities;

/// <summary>
/// A reusable paper / document template. Versioned the same way as
/// <see cref="Document"/>: new edits produce a new <see cref="TemplateVersion"/>
/// row.
/// </summary>
public class Template
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// ASP.NET Identity user id of the owner. See <see cref="Project.OwnerUserId"/>.
    /// </summary>
    public string OwnerUserId { get; set; } = string.Empty;

    /// <summary>
    /// True if visible to other users for cloning. Default false.
    /// </summary>
    public bool IsPublic { get; set; }

    public Guid? CurrentVersionId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public TemplateVersion? CurrentVersion { get; set; }
    public ICollection<TemplateVersion> Versions { get; set; } = new List<TemplateVersion>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
