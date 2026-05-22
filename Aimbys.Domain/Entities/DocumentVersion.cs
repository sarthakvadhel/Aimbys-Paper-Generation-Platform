namespace Aimbys.Domain.Entities;

/// <summary>
/// An immutable snapshot of a <see cref="Document"/>'s content. New edits
/// produce a new <see cref="DocumentVersion"/> rather than mutating an
/// existing row, so we always have an audit trail.
/// </summary>
public class DocumentVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DocumentId { get; set; }

    /// <summary>
    /// Monotonic version number scoped to <see cref="DocumentId"/>. Unique
    /// together with <see cref="DocumentId"/> via index.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Sanitized HTML produced by the editor. Stored as nvarchar(max).
    /// </summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// ASP.NET Identity user id of the author. See <see cref="Project.OwnerUserId"/>.
    /// </summary>
    public string AuthorUserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Document? Document { get; set; }
}
