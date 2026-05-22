using Aimbys.Domain.Entities;
using Aimbys.Infrastructure.Persistence;
using Aimbys.Web.ViewModels.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Controllers;

/// <summary>
/// CRUD for <see cref="Project"/>, scoped to the signed-in user. Cross-user
/// access returns <c>404 Not Found</c> rather than <c>403 Forbidden</c> so
/// the existence of someone else's project is never disclosed.
/// </summary>
[Authorize]
public class ProjectsController : Controller
{
    private const int MaxIndexResults = 100;

    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        AppDbContext db,
        UserManager<IdentityUser> userManager,
        ILogger<ProjectsController> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    // ---------- Index ----------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Index(string? q = null, bool includeArchived = false)
    {
        var userId = _userManager.GetUserId(User)!;

        var query = _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerUserId == userId);

        if (!includeArchived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var trimmed = q.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{trimmed}%") ||
                (p.Description != null && EF.Functions.Like(p.Description, $"%{trimmed}%")));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.UpdatedAtUtc)
            .Take(MaxIndexResults)
            .Select(p => new ProjectListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsArchived = p.IsArchived,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc
            })
            .ToListAsync();

        return View(new ProjectIndexViewModel
        {
            Query = q,
            IncludeArchived = includeArchived,
            Projects = items,
            TotalCount = total
        });
    }

    // ---------- Details --------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == id && p.OwnerUserId == userId)
            .Select(p => new ProjectDetailsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsArchived = p.IsArchived,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc,
                DocumentCount = p.Documents.Count
            })
            .FirstOrDefaultAsync();

        if (project is null)
        {
            return NotFound();
        }

        return View(project);
    }

    // ---------- Create ---------------------------------------------------

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProjectFormViewModel { IsEdit = false });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectFormViewModel model)
    {
        // The "IsArchived" field doesn't apply to creation; ignore whatever
        // came on the wire.
        ModelState.Remove(nameof(model.IsArchived));

        if (!ModelState.IsValid)
        {
            model.IsEdit = false;
            return View(model);
        }

        var userId = _userManager.GetUserId(User)!;
        var now = DateTime.UtcNow;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = model.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            OwnerUserId = userId,
            IsArchived = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Project created. ProjectId={ProjectId} OwnerUserId={OwnerUserId}",
            project.Id, userId);

        TempData["StatusMessage"] = $"Project \"{project.Name}\" created.";
        return RedirectToAction(nameof(Details), new { id = project.Id });
    }

    // ---------- Edit -----------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = _userManager.GetUserId(User)!;

        var model = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == id && p.OwnerUserId == userId)
            .Select(p => new ProjectFormViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsArchived = p.IsArchived,
                IsEdit = true
            })
            .FirstOrDefaultAsync();

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProjectFormViewModel model)
    {
        // Defend against form-level id tampering: the route id wins.
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.IsEdit = true;
            return View(model);
        }

        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerUserId == userId);

        if (project is null)
        {
            // Either non-existent or owned by someone else.
            return NotFound();
        }

        project.Name = model.Name.Trim();
        project.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        project.IsArchived = model.IsArchived;
        project.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Project updated. ProjectId={ProjectId} OwnerUserId={OwnerUserId}",
            project.Id, userId);

        TempData["StatusMessage"] = $"Project \"{project.Name}\" updated.";
        return RedirectToAction(nameof(Details), new { id = project.Id });
    }

    // ---------- Delete ---------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userManager.GetUserId(User)!;

        var model = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == id && p.OwnerUserId == userId)
            .Select(p => new ProjectDeleteViewModel
            {
                Id = p.Id,
                Name = p.Name,
                CreatedAtUtc = p.CreatedAtUtc,
                DocumentCount = p.Documents.Count
            })
            .FirstOrDefaultAsync();

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerUserId == userId);

        if (project is null)
        {
            return NotFound();
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Project deleted. ProjectId={ProjectId} OwnerUserId={OwnerUserId}",
            project.Id, userId);

        TempData["StatusMessage"] = $"Project \"{project.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
