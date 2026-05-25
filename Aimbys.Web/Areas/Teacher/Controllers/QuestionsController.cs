using Aimbys.Application.Authorization;
using Aimbys.Application.Multilingual;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = Roles.Teacher)]
public class QuestionsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IInstituteScope _scope;
    private readonly IMultilingualService _multilingual;

    public QuestionsController(
        AppDbContext db,
        IInstituteScope scope,
        IMultilingualService multilingual)
    {
        _db = db;
        _scope = scope;
        _multilingual = multilingual;
    }

    [HttpGet]
    public async Task<IActionResult> Translations(Guid questionId, CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null) return Forbid();

        var languages = await _db.Languages
            .Where(l => l.InstituteId == instituteId.Value && l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(ct);

        ViewBag.QuestionId = questionId;
        ViewBag.Languages = languages;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTranslation(
        Guid questionId,
        Guid languageId,
        string bodyHtml,
        string? instructionsHtml,
        string? optionsJson,
        CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null) return Forbid();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        await _multilingual.SaveTranslationAsync(
            questionId, languageId, bodyHtml, instructionsHtml, optionsJson, userId, ct);

        TempData["Success"] = "Translation saved.";
        return RedirectToAction(nameof(Translations), new { questionId });
    }
}
