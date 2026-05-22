namespace Aimbys.Domain.Entities;

/// <summary>
/// A document inside a <see cref="Project"/>. The current rendered content
/// lives in <see cref="DocumentVersion"/> records; <see cref="CurrentVersionId"/>
/// points at the active one.
/// </summary>
public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// FK to the <see cref="DocumentVersion"/> currently shown for this
    /// document. Nullable so a document can exist before its first version
    /// is saved.
    /// </summary>
    public Guid? CurrentVersionId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Project? Project { get; set; }
    public DocumentVersion? CurrentVersion { get; set; }
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    public ICollection<ExportArtifact> Exports { get; set; } = new List<ExportArtifact>();
}
