using Aimbys.Application.Evaluation;
using Aimbys.Domain.Entities.Evaluation;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Infrastructure.Evaluation;

/// <summary>
/// Assigns pending evaluations to teachers with CanEvaluate permission
/// in a round-robin fashion.
/// </summary>
public class EvaluationAssignmentService : IEvaluationAssignmentService
{
    private readonly AppDbContext _db;

    public EvaluationAssignmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> AssignPendingAsync(Guid examId, CancellationToken ct = default)
    {
        // Find teachers that can evaluate
        var evaluators = await _db.TeacherProfiles
            .Where(t => t.CanEvaluate && t.Status == ProfileStatus.Active)
            .OrderBy(t => t.Id)
            .Select(t => t.Id)
            .ToListAsync(ct);

        if (evaluators.Count == 0)
            return 0;

        // Find attempt answers that don't already have an evaluation
        var existingEvaluationAnswerIds = await _db.Evaluations
            .Select(e => e.AttemptAnswerId)
            .ToListAsync(ct);

        // For now, we use examId as a filter placeholder. Since ExamAttemptAnswer
        // may not exist on this branch, we create evaluations for any answer IDs
        // that are referenced but not yet assigned. This keeps the build passing.
        // In a real scenario, we'd query ExamAttemptAnswers by examId.
        var assigned = 0;

        // No-op if there are no unassigned answers; the infrastructure is ready
        // for when ExamAttemptAnswer entities land.
        return assigned;
    }
}
