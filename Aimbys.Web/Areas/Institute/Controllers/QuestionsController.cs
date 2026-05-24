using Aimbys.Application.Authorization;
using Aimbys.Application.Questions;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Institute.Controllers;

[Area("Institute")]
[Authorize(Roles = Roles.InstituteAdmin)]
public class QuestionsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IInstituteScope _scope;
    private readonly IQuestionAnalyticsService _analytics;

    public QuestionsController(
        AppDbContext db,
        IInstituteScope scope,
        IQuestionAnalyticsService analytics)
    {
        _db = db;
        _scope = scope;
        _analytics = analytics;
    }

    [HttpGet]
    public async Task<IActionResult> Analytics(Guid questionId, CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null) return Forbid();

        var summary = await _analytics.GetUsageSummaryAsync(questionId, ct);
        var risk = await _analytics.GetExposureRiskAsync(questionId, ct);

        var exposureLogs = await _db.QuestionExposureLogs
            .Where(e => e.QuestionId == questionId && e.InstituteId == instituteId.Value)
            .OrderByDescending(e => e.ExposedAtUtc)
            .Take(50)
            .ToListAsync(ct);

        ViewBag.QuestionId = questionId;
        ViewBag.Summary = summary;
        ViewBag.Risk = risk;
        ViewBag.ExposureLogs = exposureLogs;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> UsageChartData(Guid questionId, CancellationToken ct)
    {
        var rows = await _db.QuestionUsageAnalytics
            .Where(a => a.QuestionId == questionId)
            .OrderBy(a => a.ComputedAtUtc)
            .ToListAsync(ct);

        var labels = rows.Select(r => r.ComputedAtUtc.ToString("yyyy-MM-dd")).ToArray();
        var data = rows.Select(r => r.PapersUsedIn).ToArray();

        return Json(new
        {
            labels,
            datasets = new[]
            {
                new { label = "Papers Used", data }
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> DifficultyAuditData(Guid questionId, CancellationToken ct)
    {
        var rows = await _db.QuestionDifficultyAudits
            .Where(a => a.QuestionId == questionId)
            .OrderBy(a => a.ComputedAtUtc)
            .ToListAsync(ct);

        var labels = rows.Select(r => r.ComputedAtUtc.ToString("yyyy-MM-dd")).ToArray();
        var data = rows.Select(r => r.ConfidencePercent).ToArray();

        return Json(new
        {
            labels,
            datasets = new[]
            {
                new { label = "P-Value", data }
            }
        });
    }
}
