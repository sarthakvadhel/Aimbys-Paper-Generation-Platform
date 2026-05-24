namespace Aimbys.Domain.Entities.Questions;

/// <summary>
/// Immutable content snapshot for a <see cref="Question"/>. Each edit
/// creates a new version; the approval workflow validates against the
/// latest version's fields.
/// </summary>
using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities.Questions;

public class QuestionVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public int VersionNumber { get; set; } = 1;

    /// <summary>Rich-text body of the question (HTML).</summary>
    public string BodyHtml { get; set; } = string.Empty;

    /// <summary>Maximum marks awarded for a correct answer.</summary>
    public int Marks { get; set; }

    /// <summary>Optional difficulty tag (e.g. Easy / Medium / Hard).</summary>
    public string? DifficultyTag { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Question? Question { get; set; }
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
