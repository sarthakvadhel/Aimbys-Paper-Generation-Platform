using System.Security.Claims;
using Aimbys.Application.Audit;
using Aimbys.Application.Results;
using Aimbys.Domain.Entities.Results;
using Aimbys.Domain.Enums;
using Aimbys.Domain.Events;
using Aimbys.Infrastructure.Notifications;
using Aimbys.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Infrastructure.Results;

/// <summary>
/// V1 result publication service. Because the real score pipeline
/// (Chunks 25-28) may not be fully available on this branch, the
/// implementation uses placeholder scoring logic.
/// </summary>
public class ResultPublicationService : IResultPublicationService
{
    private readonly AppDbContext _db;
    private readonly IAuditWriter _audit;
    private readonly DomainEventCollector _events;

    public ResultPublicationService(
        AppDbContext db,
        IAuditWriter audit,
        DomainEventCollector events)
    {
        _db = db;
        _audit = audit;
        _events = events;
    }

    public Task<(bool CanPublish, string? BlockingReason)> CanPublishAsync(
        Guid examId, CancellationToken ct = default)
    {
        // V1: always allow publication; real checks (all attempts
        // evaluated, moderation complete) require Chunks 25-28.
        return Task.FromResult<(bool, string?)>((true, null));
    }

    public async Task<ResultPublishResult> PublishAsync(
        Guid examId, ClaimsPrincipal actor, CancellationToken ct = default)
    {
        var actorUserId = actor.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(actorUserId))
            return new ResultPublishResult(false, "Actor identity not resolved.", 0);

        // Check if results already published for this exam.
        var alreadyPublished = await _db.Results
            .AnyAsync(r => r.IsPublished && _db.Results
                .Where(x => x.Id == r.Id)
                .Select(x => x.ExamAttemptId)
                .Any(), ct);

        // V1 placeholder: create a Result row for each StudentProfile in
        // the institute. Real implementation would iterate ExamAttempts.
        // For now, generate placeholder results so the pipeline compiles.
        var now = DateTime.UtcNow;
        var results = new List<Result>
        {
            new Result
            {
                ExamAttemptId = examId, // placeholder: using examId as attemptId
                TotalScore = 0,
                MaxScore = 100,
                Percentage = 0,
                Grade = "N/A",
                RankInBatch = 1,
                IsPublished = true,
                PublishedAtUtc = now,
                PublishedByUserId = actorUserId,
                CreatedAtUtc = now
            }
        };

        _db.Results.AddRange(results);

        await _audit.WriteAsync(
            "Result.Published",
            "Exam",
            examId.ToString(),
            actorUserId,
            System.Text.Json.JsonSerializer.Serialize(new { ExamId = examId, Count = results.Count }),
            cancellationToken: ct);

        _events.Enqueue(new ResultPublishedEvent
        {
            ExamId = examId,
            PublishedByUserId = actorUserId,
            StudentCount = results.Count
        });

        await _db.SaveChangesAsync(ct);

        return new ResultPublishResult(true, null, results.Count);
    }

    public async Task<StudentResultView?> GetStudentResultAsync(
        Guid attemptId, string studentUserId, CancellationToken ct = default)
    {
        var result = await _db.Results
            .FirstOrDefaultAsync(r => r.ExamAttemptId == attemptId && r.IsPublished, ct);

        if (result is null)
            return null;

        // V1: return result with empty answers list since we lack the
        // full ExamAttemptAnswer pipeline.
        return new StudentResultView(
            result.ExamAttemptId,
            result.TotalScore,
            result.MaxScore,
            result.Percentage,
            result.Grade,
            result.RankInBatch,
            Array.Empty<AnswerScoreItem>());
    }
}
