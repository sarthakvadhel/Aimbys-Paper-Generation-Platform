using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities.Questions;

/// <summary>
/// Root aggregate for a question in the question bank. Tracks its
/// lifecycle status and links to versioned content via <see cref="QuestionVersion"/>.
/// </summary>
public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstituteId { get; set; }
    public Guid SubjectId { get; set; }

    /// <summary>Identity user id of the author who created this question.</summary>
    public string AuthorUserId { get; set; } = string.Empty;

    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Guid? ChapterId { get; set; }
    public Guid AuthorTeacherProfileId { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;
    public QuestionType Type { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<QuestionVersion> Versions { get; set; } = new List<QuestionVersion>();
}
