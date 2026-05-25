using System.Security.Claims;
using System.Text;
using Aimbys.Application.Audit;
using Aimbys.Application.Authorization;
using Aimbys.Application.Questions;
using Aimbys.Domain.Entities.Questions;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Aimbys.Web.ViewModels.Questions;
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
    private readonly IAuditWriter _audit;

    public QuestionsController(
        AppDbContext db,
        IQuestionLifecycleService lifecycle,
        IInstituteScope scope,
        IAuditWriter audit)
    {
        _db = db;
        _lifecycle = lifecycle;
        _scope = scope;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null)
            return View(new QuestionIndexViewModel());

        var questions = await _db.Set<Question>()
            .Where(q => q.InstituteId == instituteId.Value)
            .OrderByDescending(q => q.CreatedAtUtc)
            .Join(_db.Subjects, q => q.SubjectId, s => s.Id, (q, s) => new { q, SubjectName = s.Name })
            .Select(x => new QuestionRowViewModel
            {
                Id = x.q.Id,
                Type = x.q.Type,
                Status = x.q.Status,
                SubjectName = x.SubjectName,
                Difficulty = _db.Set<QuestionVersion>()
                    .Where(v => v.QuestionId == x.q.Id && v.IsCurrentVersion)
                    .Select(v => v.Difficulty)
                    .FirstOrDefault(),
                Marks = _db.Set<QuestionVersion>()
                    .Where(v => v.QuestionId == x.q.Id && v.IsCurrentVersion)
                    .Select(v => v.Marks)
                    .FirstOrDefault(),
                CreatedAtUtc = x.q.CreatedAtUtc,
                BodyPreview = _db.Set<QuestionVersion>()
                    .Where(v => v.QuestionId == x.q.Id && v.IsCurrentVersion)
                    .Select(v => v.BodyHtml.Substring(0, 100))
                    .FirstOrDefault() ?? string.Empty
            })
            .ToListAsync(ct);

        return View(new QuestionIndexViewModel { Questions = questions });
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Import(IFormFile? file)
    {
        // Stub: import functionality will be implemented in a later chunk
        TempData["Info"] = "Import functionality is not yet implemented.";
        return RedirectToAction(nameof(Index));
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

    [HttpGet]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null)
            return Forbid();

        var questions = await _db.Set<Question>()
            .Where(q => q.InstituteId == instituteId.Value)
            .OrderByDescending(q => q.CreatedAtUtc)
            .Join(_db.Subjects, q => q.SubjectId, s => s.Id, (q, s) => new { q, SubjectName = s.Name })
            .Select(x => new
            {
                x.q.Id,
                x.q.Type,
                x.q.Status,
                x.SubjectName,
                x.q.CreatedAtUtc
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Type,Status,Subject,CreatedAtUtc");
        foreach (var q in questions)
        {
            sb.AppendLine($"\"{q.Id}\",\"{q.Type}\",\"{q.Status}\",\"{EscapeCsv(q.SubjectName)}\",\"{q.CreatedAtUtc:O}\"");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _audit.WriteAsync(
            "Questions.Exported",
            "Question",
            instituteId.Value.ToString(),
            userId,
            $"{{\"count\":{questions.Count}}}",
            AuditSeverity.Information,
            ct);
        await _db.SaveChangesAsync(ct);

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"questions-export-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string EscapeCsv(string value) => value.Replace("\"", "\"\"");
}
