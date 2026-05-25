using Aimbys.Application.Analytics;
using Aimbys.Domain.Entities.Analytics;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Aimbys.Infrastructure.Analytics;

/// <summary>
/// V1 stub implementation of <see cref="IAnalyticsAggregationService"/>.
/// Each method logs a placeholder message and writes an AnalyticsSnapshot
/// row so the pipeline is exercised end-to-end. Real queries require
/// exam/result data that lands when Chunks 25-29 merge to main.
/// </summary>
public sealed class AnalyticsAggregationService : IAnalyticsAggregationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AnalyticsAggregationService> _logger;

    public AnalyticsAggregationService(AppDbContext db, ILogger<AnalyticsAggregationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task AggregateInstituteMetricsAsync(Guid instituteId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Aggregation for Institute {InstituteId} — no source data yet (lands when Chunks 25-29 merge to main).",
            instituteId);

        _db.AnalyticsSnapshots.Add(new AnalyticsSnapshot
        {
            Scope = AnalyticsScope.Institute,
            ScopeId = instituteId,
            MetricKey = "institute.metrics.stub",
            MetricValueJson = "\"pending\"",
            CapturedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task AggregateStudentPerformanceAsync(Guid instituteId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Aggregation for Student performance in Institute {InstituteId} — no source data yet (lands when Chunks 25-29 merge to main).",
            instituteId);

        _db.AnalyticsSnapshots.Add(new AnalyticsSnapshot
        {
            Scope = AnalyticsScope.Student,
            ScopeId = instituteId,
            MetricKey = "student.performance.stub",
            MetricValueJson = "\"pending\"",
            CapturedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task AggregateEvaluatorEfficiencyAsync(Guid instituteId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Aggregation for Evaluator efficiency in Institute {InstituteId} — no source data yet (lands when Chunks 25-29 merge to main).",
            instituteId);

        _db.AnalyticsSnapshots.Add(new AnalyticsSnapshot
        {
            Scope = AnalyticsScope.Institute,
            ScopeId = instituteId,
            MetricKey = "evaluator.efficiency.stub",
            MetricValueJson = "\"pending\"",
            CapturedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task RecomputeLeaderboardAsync(Guid examId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Aggregation for Exam leaderboard {ExamId} — no source data yet (lands when Chunks 25-29 merge to main).",
            examId);

        _db.AnalyticsSnapshots.Add(new AnalyticsSnapshot
        {
            Scope = AnalyticsScope.Exam,
            ScopeId = examId,
            MetricKey = "leaderboard.recompute.stub",
            MetricValueJson = "\"pending\"",
            CapturedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }
}
