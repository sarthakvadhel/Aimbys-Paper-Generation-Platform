using System.Text.Json;
using Aimbys.Application.Exams;
using Aimbys.Domain.Entities.Exams;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aimbys.Infrastructure.Exams;

/// <summary>
/// Manages the runtime lifecycle of an exam attempt: start, save
/// answers, flag questions, and submit (with auto-evaluation for
/// objective question types).
/// </summary>
public sealed class ExamRuntimeService : IExamRuntimeService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ExamRuntimeService> _logger;

    public ExamRuntimeService(AppDbContext db, ILogger<ExamRuntimeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ExamAttemptResult> StartAttemptAsync(Guid examId, Guid studentProfileId, CancellationToken ct = default)
    {
        var exam = await _db.Exams
            .FirstOrDefaultAsync(e => e.Id == examId, ct);

        if (exam == null)
            return new ExamAttemptResult(false, "Exam not found.");

        if (exam.Status != ExamStatus.Live && exam.Status != ExamStatus.Scheduled)
            return new ExamAttemptResult(false, "Exam is not available.");

        // Check student belongs to the class batch
        var studentInBatch = await _db.StudentProfiles
            .AnyAsync(sp => sp.Id == studentProfileId && sp.ClassBatchId == exam.ClassBatchId, ct);
        if (!studentInBatch)
            return new ExamAttemptResult(false, "Student is not in the exam's class batch.");

        // Ensure no existing attempt
        var existingAttempt = await _db.ExamAttempts
            .AnyAsync(a => a.ExamId == examId && a.StudentProfileId == studentProfileId, ct);
        if (existingAttempt)
            return new ExamAttemptResult(false, "An attempt already exists for this student.");

        var attempt = new ExamAttempt
        {
            ExamId = examId,
            StudentProfileId = studentProfileId,
            Status = AttemptStatus.InProgress,
            StartedAtUtc = DateTime.UtcNow
        };

        _db.ExamAttempts.Add(attempt);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Exam attempt {AttemptId} started for student {StudentProfileId} on exam {ExamId}.",
            attempt.Id, studentProfileId, examId);

        return new ExamAttemptResult(true, AttemptId: attempt.Id);
    }

    public async Task<SaveAnswerResult> SaveAnswerAsync(Guid attemptId, Guid questionId, string? answerJson, string studentUserId, CancellationToken ct = default)
    {
        var attempt = await _db.ExamAttempts
            .Include(a => a.Exam)
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt?.Exam == null)
            return new SaveAnswerResult(false, "Attempt not found.");

        if (attempt.Status != AttemptStatus.InProgress)
            return new SaveAnswerResult(false, "Attempt is not in progress.");

        // Timer check
        if (attempt.StartedAtUtc.HasValue)
        {
            var deadline = attempt.StartedAtUtc.Value.AddMinutes(attempt.Exam.DurationMinutes);
            if (DateTime.UtcNow > deadline)
                return new SaveAnswerResult(false, "timer_expired");
        }

        // Upsert answer
        var answer = await _db.ExamAttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == questionId, ct);

        if (answer == null)
        {
            answer = new ExamAttemptAnswer
            {
                AttemptId = attemptId,
                QuestionId = questionId,
                QuestionVersionId = questionId, // simplified: same as questionId
                AnswerJson = answerJson,
                LastSavedAtUtc = DateTime.UtcNow
            };
            _db.ExamAttemptAnswers.Add(answer);
        }
        else
        {
            answer.AnswerJson = answerJson;
            answer.LastSavedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return new SaveAnswerResult(true);
    }

    public async Task<bool> FlagQuestionAsync(Guid attemptId, Guid questionId, bool flagged, string studentUserId, CancellationToken ct = default)
    {
        var answer = await _db.ExamAttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == questionId, ct);

        if (answer == null)
        {
            // Create a placeholder answer row so flag state is tracked
            answer = new ExamAttemptAnswer
            {
                AttemptId = attemptId,
                QuestionId = questionId,
                QuestionVersionId = questionId,
                IsFlagged = flagged,
                LastSavedAtUtc = DateTime.UtcNow
            };
            _db.ExamAttemptAnswers.Add(answer);
        }
        else
        {
            answer.IsFlagged = flagged;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SubmitResult> SubmitAsync(Guid attemptId, string studentUserId, CancellationToken ct = default)
    {
        var attempt = await _db.ExamAttempts
            .Include(a => a.Exam)
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt?.Exam == null)
            return new SubmitResult(false, "Attempt not found.");

        if (attempt.Status != AttemptStatus.InProgress)
            return new SubmitResult(false, "Attempt is not in progress.");

        attempt.Status = AttemptStatus.Submitted;
        attempt.SubmittedAtUtc = DateTime.UtcNow;

        // Simple auto-evaluation: for now just sum up any AutoMarksAwarded
        // that were set externally (full MCQ auto-eval deferred to when
        // question metadata is available).
        decimal totalAutoScore = 0;
        foreach (var ans in attempt.Answers)
        {
            totalAutoScore += ans.AutoMarksAwarded ?? 0;
        }
        attempt.TotalAutoScore = totalAutoScore;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Exam attempt {AttemptId} submitted with auto-score {Score}.",
            attemptId, totalAutoScore);

        return new SubmitResult(true, TotalAutoScore: totalAutoScore);
    }
}
