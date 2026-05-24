using Aimbys.Application.Authorization;
using Aimbys.Application.Questions;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Institute.Controllers;

[Area("Institute")]
[Authorize(Roles = Roles.InstituteAdmin + "," + Roles.Teacher)]
public class QuestionsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IQuestionLifecycleService _lifecycle;
    private readonly IInstituteScope _scope;

    public QuestionsController(
        AppDbContext db,
        IQuestionLifecycleService lifecycle,
        IInstituteScope scope)
    {
        _db = db;
        _lifecycle = lifecycle;
        _scope = scope;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null) return Forbid();

        var questions = await _db.Questions
            .Where(q => q.InstituteId == instituteId.Value)
            .OrderByDescending(q => q.UpdatedAtUtc)
            .ToListAsync(ct);

        return View(questions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignReviewer(Guid questionId, Guid reviewerProfileId, CancellationToken ct)
    {
        var result = await _lifecycle.AssignReviewerAsync(questionId, reviewerProfileId, User, ct);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Reviewer assigned.";
        return RedirectToAction(nameof(Index));
    }
}
