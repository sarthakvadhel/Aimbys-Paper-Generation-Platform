namespace Aimbys.Domain.Entities.Exams;

/// <summary>
/// Represents a scheduled examination. Minimal entity introduced in Chunk 25/26;
/// properties will be extended in subsequent chunks.
/// </summary>
public class Exam
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstituteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
