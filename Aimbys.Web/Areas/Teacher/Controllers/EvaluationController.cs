using Aimbys.Application.Evaluation;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Aimbys.Web.Areas.Teacher.Controllers;

/// <summary>
/// Evaluation desk: teacher views pending evaluations, scores answers
/// against rubrics, and submits final evaluated scores.
/// </summary>
[Area("Teacher")]
[Authorize(Roles = Roles.Teacher)]
public class EvaluationController : Controller
{
    private readonly IEvaluationService _evaluationService;
    private readonly AppDbContext _db;

    public EvaluationController(
        IEvaluationService evaluationService,
        AppDbContext db)
    {
        _evaluationService = evaluationService;
        _db = db;
    }

    /// <summary>Evaluator inbox — list evaluations filtered by status.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? status = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // Get teacher profile for current user
        var teacherProfile = await _db.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacherProfile is null)
            return View(new List<Domain.Entities.Evaluation.Evaluation>());

        var query = _db.Evaluations
            .Where(e => e.EvaluatorTeacherProfileId == teacherProfile.Id);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Domain.Enums.EvaluationStatus>(status, out var parsed))
        {
            query = query.Where(e => e.Status == parsed);
        }

        var evaluations = await query
            .OrderByDescending(e => e.AssignedAtUtc)
            .ToListAsync();

        return View(evaluations);
    }

    /// <summary>Split-view scoring page for a single evaluation.</summary>
    [HttpGet]
    public async Task<IActionResult> Open(Guid evaluationId)
    {
        var context = await _evaluationService.GetScoringContextAsync(evaluationId);
        if (context is null) return NotFound();
        return View(context);
    }

    /// <summary>Autosave endpoint for draft criterion scores.</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SaveDraftScore([FromBody] SaveDraftScoreRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var ok = await _evaluationService.SaveDraftScoreAsync(
            request.EvaluationId, request.CriterionIndex, request.Points, userId);
        return ok ? Ok() : BadRequest();
    }

    /// <summary>Autosave endpoint for feedback text.</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SaveFeedback([FromBody] SaveFeedbackRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var ok = await _evaluationService.SaveFeedbackDraftAsync(
            request.EvaluationId, request.Feedback, userId);
        return ok ? Ok() : BadRequest();
    }

    /// <summary>Submit evaluation — finalizes scores and transitions status.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Guid evaluationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _evaluationService.SubmitAsync(evaluationId, userId);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Open), new { evaluationId });
        }

        TempData["Success"] = "Evaluation submitted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

// ----- Request DTOs for JSON endpoints ----------------------------------
public record SaveDraftScoreRequest(Guid EvaluationId, int CriterionIndex, decimal Points);
public record SaveFeedbackRequest(Guid EvaluationId, string Feedback);
