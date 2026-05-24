using Aimbys.Domain.Entities.Exams;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Institute.Controllers;

[Area("Institute")]
[Authorize(Roles = Roles.InstituteAdmin)]
public class ExamsController : Controller
{
    private readonly AppDbContext _db;

    public ExamsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> SecurityProfile(Guid examId)
    {
        var profile = await _db.ExamSecurityProfiles
            .FirstOrDefaultAsync(p => p.ExamId == examId);

        if (profile is null)
        {
            profile = new ExamSecurityProfile { ExamId = examId };
        }

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SecurityProfile(ExamSecurityProfile model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existing = await _db.ExamSecurityProfiles
            .FirstOrDefaultAsync(p => p.ExamId == model.ExamId);

        if (existing is null)
        {
            _db.ExamSecurityProfiles.Add(model);
        }
        else
        {
            existing.RequireFullscreen = model.RequireFullscreen;
            existing.DetectTabSwitch = model.DetectTabSwitch;
            existing.DetectResize = model.DetectResize;
            existing.BlockCopyPaste = model.BlockCopyPaste;
            existing.BlockKeyboardShortcuts = model.BlockKeyboardShortcuts;
            existing.HeartbeatIntervalSeconds = model.HeartbeatIntervalSeconds;
            existing.MaxConnectionLossSeconds = model.MaxConnectionLossSeconds;
            existing.AutoSubmitOnTimeout = model.AutoSubmitOnTimeout;
            existing.TrackDevice = model.TrackDevice;
            existing.TrackSession = model.TrackSession;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Security profile saved.";
        return RedirectToAction(nameof(SecurityProfile), new { examId = model.ExamId });
    }
}
