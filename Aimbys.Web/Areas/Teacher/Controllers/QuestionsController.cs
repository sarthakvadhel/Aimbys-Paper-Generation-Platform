using Aimbys.Application.Questions;
using Aimbys.Application.Storage;
using Aimbys.Domain.Entities.Questions;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Aimbys.Web.ViewModels.Questions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = Roles.Teacher)]
public class QuestionsController : Controller
{
    private readonly IQuestionAuthoringService _authoring;
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IFileStorageService _fileStorage;

    public QuestionsController(
        IQuestionAuthoringService authoring,
        AppDbContext db,
        UserManager<IdentityUser> userManager,
        IFileStorageService fileStorage)
    {
        _authoring = authoring;
        _db = db;
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = _userManager.GetUserId(User);
        var teacherProfile = await _db.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);

        if (teacherProfile is null)
            return View(new QuestionIndexViewModel());

        var questions = await _db.Set<Question>()
            .Where(q => q.AuthorTeacherProfileId == teacherProfile.Id)
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
    public IActionResult Create()
    {
        return View(new QuestionCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionCreateViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var request = new QuestionCreateRequest
        {
            Type = model.Type,
            SubjectId = model.SubjectId,
            ChapterId = model.ChapterId,
            BodyHtml = model.BodyHtml,
            Difficulty = model.Difficulty,
            BloomLevel = model.BloomLevel,
            Marks = model.Marks,
            EstimatedTimeMinutes = model.EstimatedTimeMinutes,
            InstructionsHtml = model.InstructionsHtml,
            Options = model.Options.Select(o => new OptionInput(o.Label, o.Text, o.IsCorrect, o.SortOrder)).ToList(),
            RubricCriteria = model.RubricCriteria.Select(r => new RubricInput(r.Criterion, r.MaxPoints, r.SortOrder)).ToList(),
            TestCases = model.TestCases.Select(t => new TestCaseInput(t.Input, t.ExpectedOutput, t.IsHidden, t.TimeoutMs, t.MemoryLimitMb, t.SortOrder)).ToList()
        };

        var result = await _authoring.CreateAsync(request, User, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create question.");
            return View(model);
        }

        TempData["Success"] = "Question created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var question = await _db.Set<Question>()
            .Include(q => q.Versions.Where(v => v.IsCurrentVersion))
                .ThenInclude(v => v.Options)
            .Include(q => q.Versions.Where(v => v.IsCurrentVersion))
                .ThenInclude(v => v.RubricCriteria)
            .Include(q => q.Versions.Where(v => v.IsCurrentVersion))
                .ThenInclude(v => v.TestCases)
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        if (question is null)
            return NotFound();

        var currentVersion = question.Versions.FirstOrDefault(v => v.IsCurrentVersion);
        if (currentVersion is null)
            return NotFound();

        var model = new QuestionEditViewModel
        {
            QuestionId = question.Id,
            Type = question.Type,
            Status = question.Status,
            BodyHtml = currentVersion.BodyHtml,
            Difficulty = currentVersion.Difficulty,
            BloomLevel = currentVersion.BloomLevel,
            Marks = currentVersion.Marks,
            EstimatedTimeMinutes = currentVersion.EstimatedTimeMinutes,
            InstructionsHtml = currentVersion.InstructionsHtml,
            Options = currentVersion.Options.OrderBy(o => o.SortOrder).Select(o => new OptionViewModel
            {
                Label = o.Label,
                Text = o.Text,
                IsCorrect = o.IsCorrect,
                SortOrder = o.SortOrder
            }).ToList(),
            RubricCriteria = currentVersion.RubricCriteria.OrderBy(r => r.SortOrder).Select(r => new RubricViewModel
            {
                Criterion = r.Criterion,
                MaxPoints = r.MaxPoints,
                SortOrder = r.SortOrder
            }).ToList(),
            TestCases = currentVersion.TestCases.OrderBy(t => t.SortOrder).Select(t => new TestCaseViewModel
            {
                Input = t.Input,
                ExpectedOutput = t.ExpectedOutput,
                IsHidden = t.IsHidden,
                TimeoutMs = t.TimeoutMs,
                MemoryLimitMb = t.MemoryLimitMb,
                SortOrder = t.SortOrder
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, QuestionEditViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var request = new QuestionEditRequest
        {
            BodyHtml = model.BodyHtml,
            Difficulty = model.Difficulty,
            BloomLevel = model.BloomLevel,
            Marks = model.Marks,
            EstimatedTimeMinutes = model.EstimatedTimeMinutes,
            InstructionsHtml = model.InstructionsHtml,
            Options = model.Options.Select(o => new OptionInput(o.Label, o.Text, o.IsCorrect, o.SortOrder)).ToList(),
            RubricCriteria = model.RubricCriteria.Select(r => new RubricInput(r.Criterion, r.MaxPoints, r.SortOrder)).ToList(),
            TestCases = model.TestCases.Select(t => new TestCaseInput(t.Input, t.ExpectedOutput, t.IsHidden, t.TimeoutMs, t.MemoryLimitMb, t.SortOrder)).ToList()
        };

        var result = await _authoring.EditAsync(id, request, User, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to edit question.");
            return View(model);
        }

        TempData["Success"] = result.NewVersionCreated
            ? "New version created successfully."
            : "Question updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RevisionHistory(Guid id, CancellationToken ct)
    {
        var history = await _authoring.GetRevisionHistoryAsync(id, ct);
        ViewBag.QuestionId = id;
        return View(history);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAsset(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var mimeAllowList = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        const long maxBytes = 5 * 1024 * 1024; // 5 MB

        try
        {
            var result = await _fileStorage.SaveAsync(
                FileArea.Questions,
                $"QuestionAsset:{Guid.NewGuid()}",
                file,
                mimeAllowList,
                maxBytes,
                instituteId: null,
                User,
                ct);

            return Json(new { location = $"/files/{result.Token}" });
        }
        catch (Application.Storage.FileStorageException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
