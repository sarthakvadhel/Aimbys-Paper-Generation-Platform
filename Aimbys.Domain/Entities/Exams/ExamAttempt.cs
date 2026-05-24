namespace Aimbys.Domain.Entities.Exams;

/// <summary>
/// Represents a student's attempt at an exam. Introduced in Chunk 25/26.
/// </summary>
public class ExamAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExamId { get; set; }
    public string StudentUserId { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }
    public bool IsSubmitted { get; set; }
    public bool IsSuspicious { get; set; }
    public Exam? Exam { get; set; }
}
