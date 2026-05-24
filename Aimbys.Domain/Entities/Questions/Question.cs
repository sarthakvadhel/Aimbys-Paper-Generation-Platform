using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities.Questions;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstituteId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? ChapterId { get; set; }
    public Guid AuthorTeacherProfileId { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;
    public QuestionType Type { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<QuestionVersion> Versions { get; set; } = new List<QuestionVersion>();
}
