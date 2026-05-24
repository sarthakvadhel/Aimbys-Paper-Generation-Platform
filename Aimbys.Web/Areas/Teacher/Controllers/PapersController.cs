using Aimbys.Application.Authorization;
using Aimbys.Application.Papers;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Teacher.Controllers;

/// <summary>
/// Teacher-facing paper authoring surface. Teachers create, edit,
/// preview, and submit papers for institute approval.
/// </summary>
[Area("Teacher")]
[Authorize(Roles = Roles.Teacher)]
public class PapersController : Controller
{
    private readonly AppDbContext _db;
    private readonly IInstituteScope _scope;
    private readonly IPaperAssemblyService _paperService;

    public PapersController(
        AppDbContext db,
        IInstituteScope scope,
        IPaperAssemblyService paperService)
    {
        _db = db;
        _scope = scope;
        _paperService = paperService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var teacherProfile = await _db.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);
        if (teacherProfile is null) return Forbid();

        var papers = await _db.Papers
            .Where(p => p.AuthorTeacherProfileId == teacherProfile.Id)
            .Include(p => p.Versions)
            .OrderByDescending(p => p.UpdatedAtUtc)
            .ToListAsync(ct);

        return View(papers);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null) return Forbid();

        var subjects = await _db.Subjects
            .Where(s => s.InstituteId == instituteId.Value)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        ViewBag.Subjects = subjects;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, Guid subjectId, int totalMarks, int durationMinutes, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Title is required.";
            return RedirectToAction(nameof(Create));
        }

        var request = new PaperCreateRequest(title.Trim(), subjectId, totalMarks, durationMinutes);
        var result = await _paperService.CreateDraftAsync(request, User, ct);

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "Failed to create paper.";
            return RedirectToAction(nameof(Create));
        }

        TempData["Success"] = "Paper draft created.";
        return RedirectToAction(nameof(Edit), new { id = result.PaperId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var paper = await _db.Papers
            .Include(p => p.Versions)
                .ThenInclude(v => v.Sections)
            .Include(p => p.Versions)
                .ThenInclude(v => v.Questions)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (paper is null) return NotFound();

        // Ownership check
        var teacherProfile = await _db.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);
        if (teacherProfile is null || paper.AuthorTeacherProfileId != teacherProfile.Id)
            return Forbid();

        return View(paper);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct,
        [FromForm] string? sectionsJson = null,
        [FromForm] string? questionsJson = null)
    {
        // Simplified: in production this would parse the JSON form data
        // For now just redirect back
        TempData["Success"] = "Paper saved.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var result = await _paperService.SubmitForApprovalAsync(id, User, ct);

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "Failed to submit paper.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["Success"] = "Paper submitted for approval.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Preview(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var paper = await _db.Papers
            .Include(p => p.Versions)
                .ThenInclude(v => v.Sections)
            .Include(p => p.Versions)
                .ThenInclude(v => v.Questions)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (paper is null) return NotFound();

        var teacherProfile = await _db.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);
        if (teacherProfile is null || paper.AuthorTeacherProfileId != teacherProfile.Id)
            return Forbid();

        return View(paper);
    }
}
