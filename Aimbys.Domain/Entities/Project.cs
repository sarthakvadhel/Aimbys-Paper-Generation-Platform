namespace Aimbys.Domain.Entities;

/// <summary>
/// A workspace that groups <see cref="Document"/>s and <see cref="Job"/>s
/// belonging to a single owner.
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// ASP.NET Identity user id of the owner. Stored as a free string so the
    /// schema doesn't have to change when Identity is wired up in Chunk 3.
    /// </summary>
    public string OwnerUserId { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
