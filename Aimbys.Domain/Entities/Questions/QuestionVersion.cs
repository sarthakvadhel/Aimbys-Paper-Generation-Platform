using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities.Questions;

public class QuestionVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string BodyHtml { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public BloomLevel BloomLevel { get; set; }
    public decimal Marks { get; set; }
    public int? EstimatedTimeMinutes { get; set; }
    public string? InstructionsHtml { get; set; }
    public bool IsCurrentVersion { get; set; } = true;
    public string AuthorUserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Question? Question { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public ICollection<QuestionRubricCriterion> RubricCriteria { get; set; } = new List<QuestionRubricCriterion>();
    public ICollection<QuestionTestCase> TestCases { get; set; } = new List<QuestionTestCase>();
}
