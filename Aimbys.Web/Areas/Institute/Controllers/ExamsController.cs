using Aimbys.Application.Exams;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Institute.Controllers;

/// <summary>
/// Institute-admin exam management: calendar view and scheduling.
/// </summary>
[Area("Institute")]
[Authorize(Roles = Roles.InstituteAdmin)]
public class ExamsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IExamSchedulingService _scheduling;

    public ExamsController(AppDbContext db, IExamSchedulingService scheduling)
    {
        _db = db;
        _scheduling = scheduling;
    }

    /// <summary>GET /Institute/Exams/Calendar — list of exams.</summary>
    [HttpGet]
    public async Task<IActionResult> Calendar(CancellationToken ct)
    {
        var exams = await _db.Exams
            .OrderByDescending(e => e.ScheduledAtUtc)
            .Take(100)
            .ToListAsync(ct);

        return View(exams);
    }

    /// <summary>GET /Institute/Exams/Schedule — form.</summary>
    [HttpGet]
    public async Task<IActionResult> Schedule(CancellationToken ct)
    {
        ViewBag.ClassBatches = await _db.ClassBatches
            .OrderBy(cb => cb.Name)
            .ToListAsync(ct);

        return View();
    }

    /// <summary>POST /Institute/Exams/Schedule — create exam.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(
        Guid paperVersionId,
        Guid classBatchId,
        string title,
        DateTime scheduledAtUtc,
        int durationMinutes,
        CancellationToken ct)
    {
        var request = new ExamScheduleRequest(
            paperVersionId,
            classBatchId,
            title,
            scheduledAtUtc,
            durationMinutes);

        var result = await _scheduling.ScheduleAsync(request, User, ct);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Schedule));
        }

        TempData["Success"] = "Exam scheduled successfully.";
        return RedirectToAction(nameof(Calendar));
    }
}
