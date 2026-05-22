using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities;

/// <summary>
/// A rendered file (PDF, DOCX, HTML, MD) produced from a <see cref="Document"/>.
/// The actual bytes are not stored in the database; <see cref="StorageUri"/>
/// points at the blob.
/// </summary>
public class ExportArtifact
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DocumentId { get; set; }

    public ExportFormat Format { get; set; }

    /// <summary>
    /// URI of the rendered file (e.g. blob storage URL or file:// path).
    /// </summary>
    public string StorageUri { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    /// <summary>ASP.NET Identity user id of the requester.</summary>
    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Document? Document { get; set; }
}
