namespace Aimbys.Domain.Entities.Exams;

public class ExamAttemptAnswer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid QuestionVersionId { get; set; }
    public string? AnswerJson { get; set; }
    public bool IsFlagged { get; set; }
    public decimal? AutoMarksAwarded { get; set; }
    public DateTime? LastSavedAtUtc { get; set; }
    public ExamAttempt? Attempt { get; set; }
}
