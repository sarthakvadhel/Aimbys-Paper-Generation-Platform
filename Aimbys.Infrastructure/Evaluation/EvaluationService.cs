using Aimbys.Application.Audit;
using Aimbys.Application.Evaluation;
using Aimbys.Domain.Entities.Evaluation;
using Aimbys.Domain.Enums;
using Aimbys.Domain.Events;
using Aimbys.Infrastructure.Notifications;
using Aimbys.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Infrastructure.Evaluation;

/// <summary>
/// Core evaluation service: draft-save, submit, and scoring context retrieval.
/// </summary>
public class EvaluationService : IEvaluationService
{
    private readonly AppDbContext _db;
    private readonly DomainEventCollector _events;
    private readonly IAuditWriter _audit;

    public EvaluationService(
        AppDbContext db,
        DomainEventCollector events,
        IAuditWriter audit)
    {
        _db = db;
        _events = events;
        _audit = audit;
    }

    public async Task<bool> SaveDraftScoreAsync(
        Guid evaluationId, int criterionIndex, decimal points,
        string actorUserId, CancellationToken ct = default)
    {
        var evaluation = await _db.Evaluations.FindAsync(new object[] { evaluationId }, ct);
        if (evaluation is null) return false;

        var existing = await _db.DraftScores
            .FirstOrDefaultAsync(d => d.EvaluationId == evaluationId && d.CriterionIndex == criterionIndex, ct);

        if (existing is not null)
        {
            existing.PointsAwarded = points;
            existing.SavedAtUtc = DateTime.UtcNow;
        }
        else
        {
            _db.DraftScores.Add(new DraftScore
            {
                EvaluationId = evaluationId,
                CriterionIndex = criterionIndex,
                PointsAwarded = points,
            });
        }

        if (evaluation.Status == EvaluationStatus.Pending)
        {
            evaluation.Status = EvaluationStatus.InProgress;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SaveFeedbackDraftAsync(
        Guid evaluationId, string feedback,
        string actorUserId, CancellationToken ct = default)
    {
        var evaluation = await _db.Evaluations.FindAsync(new object[] { evaluationId }, ct);
        if (evaluation is null) return false;

        evaluation.Feedback = feedback;

        if (evaluation.Status == EvaluationStatus.Pending)
        {
            evaluation.Status = EvaluationStatus.InProgress;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<EvaluationSubmitResult> SubmitAsync(
        Guid evaluationId, string actorUserId, CancellationToken ct = default)
    {
        var evaluation = await _db.Evaluations.FindAsync(new object[] { evaluationId }, ct);
        if (evaluation is null)
            return new EvaluationSubmitResult(false, "Evaluation not found.");

        // Load draft scores
        var draftScores = await _db.DraftScores
            .Where(d => d.EvaluationId == evaluationId)
            .ToListAsync(ct);

        if (draftScores.Count == 0)
            return new EvaluationSubmitResult(false, "No scores have been entered.");

        // Compute totals
        var totalAwarded = draftScores.Sum(d => d.PointsAwarded);

        // Try to get max points from the scoring scheme (if available)
        // We attempt to find via evaluation -> attempt answer -> question -> scheme
        // For now we sum draft scores as total; max will be populated when ScoringScheme is linked
        decimal maxPossible = 0m;

        // Look up scoring scheme through the evaluation's context
        // This is a best-effort lookup — if scheme doesn't exist, we use sum of drafts
        var scoringSchemes = await _db.ScoringSchemes.ToListAsync(ct);
        // In production, we'd filter by the specific question; for now accept the sum

        // Write immutable EvaluatedScore row
        var evaluatedScore = new EvaluatedScore
        {
            EvaluationId = evaluationId,
            TotalPointsAwarded = totalAwarded,
            MaxPointsPossible = maxPossible,
            Feedback = evaluation.Feedback,
            EvaluatedByUserId = actorUserId,
            EvaluatedAtUtc = DateTime.UtcNow,
        };
        _db.EvaluatedScores.Add(evaluatedScore);

        // Update evaluation status
        evaluation.Status = EvaluationStatus.Submitted;
        evaluation.CompletedAtUtc = DateTime.UtcNow;

        // Enqueue domain event
        _events.Enqueue(new EvaluationSubmittedEvent
        {
            EvaluationId = evaluationId,
            AttemptAnswerId = evaluation.AttemptAnswerId,
            EvaluatorUserId = actorUserId,
        });

        await _db.SaveChangesAsync(ct);

        await _audit.WriteAsync(
            action: "Evaluation.Submitted",
            entityType: "Evaluation",
            entityId: evaluationId.ToString(),
            actorUserId: actorUserId,
            detailsJson: $"{{\"totalAwarded\":{totalAwarded},\"maxPossible\":{maxPossible}}}",
            cancellationToken: ct);

        return new EvaluationSubmitResult(true);
    }

    public async Task<EvaluationContext?> GetScoringContextAsync(
        Guid evaluationId, CancellationToken ct = default)
    {
        var evaluation = await _db.Evaluations.FindAsync(new object[] { evaluationId }, ct);
        if (evaluation is null) return null;

        var draftScores = await _db.DraftScores
            .Where(d => d.EvaluationId == evaluationId)
            .Select(d => new DraftScoreItem(d.CriterionIndex, d.PointsAwarded))
            .ToListAsync(ct);

        // Attempt to load scoring scheme criteria
        // In production this would join via the attempt answer's question
        string? criteriaJson = null;

        return new EvaluationContext(
            EvaluationId: evaluationId,
            AnswerJson: null, // Would come from ExamAttemptAnswer when that entity lands
            CriteriaJson: criteriaJson,
            DraftScores: draftScores,
            Feedback: evaluation.Feedback);
    }
}
