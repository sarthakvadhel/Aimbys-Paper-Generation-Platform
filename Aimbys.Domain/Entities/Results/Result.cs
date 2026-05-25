namespace Aimbys.Domain.Entities.Results;

public class Result
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExamAttemptId { get; set; }
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public double Percentage { get; set; }
    public string? Grade { get; set; }
    public int? RankInBatch { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public string? PublishedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
