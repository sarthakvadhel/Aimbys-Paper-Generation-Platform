namespace Aimbys.Domain.Entities;

/// <summary>
/// Immutable snapshot of a <see cref="Template"/>'s content. See
/// <see cref="DocumentVersion"/> for the same versioning pattern.
/// </summary>
public class TemplateVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TemplateId { get; set; }

    /// <summary>
    /// Monotonic version number scoped to <see cref="TemplateId"/>.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Sanitized HTML produced by the editor. Stored as nvarchar(max).
    /// </summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// ASP.NET Identity user id of the author.
    /// </summary>
    public string AuthorUserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Template? Template { get; set; }
}
