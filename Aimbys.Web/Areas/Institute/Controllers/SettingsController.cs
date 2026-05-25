using Aimbys.Application.Authorization;
using Aimbys.Application.Configuration;
using Aimbys.Application.Subscriptions;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Aimbys.Web.Areas.Institute.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Web.Areas.Institute.Controllers;

/// <summary>
/// Institute settings controller. Currently provides the feature toggles
/// management page where Institute Admins can enable/disable platform
/// features subject to their license tier.
/// </summary>
[Area("Institute")]
[Authorize(Roles = Roles.InstituteAdmin)]
public class SettingsController : Controller
{
    private readonly IConfigurationService _config;
    private readonly IInstituteScope _scope;
    private readonly AppDbContext _db;

    public SettingsController(
        IConfigurationService config,
        IInstituteScope scope,
        AppDbContext db)
    {
        _config = config;
        _scope = scope;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Features));
    }

    [HttpGet]
    public async Task<IActionResult> Features(CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null)
            return Forbid();

        var institute = await _db.Institutes
            .FirstOrDefaultAsync(i => i.Id == instituteId.Value, ct);

        if (institute is null)
            return NotFound();

        var toggles = new List<FeatureToggleRow>();

        foreach (var key in PlatformFeatureKeys.AllToggles)
        {
            var isEnabled = await _config.IsFeatureEnabledAsync(key, instituteId, ct);
            var isTierAllowed = LicenseTierFeatureMap.IsTierAllowed(key, institute.LicenseTier);
            var minimumTier = LicenseTierFeatureMap.GetMinimumTier(key);

            toggles.Add(new FeatureToggleRow
            {
                Key = key,
                DisplayName = FormatDisplayName(key),
                IsEnabled = isEnabled,
                IsTierAllowed = isTierAllowed,
                MinimumTier = minimumTier
            });
        }

        var model = new FeatureSettingsViewModel
        {
            CurrentTier = institute.LicenseTier,
            Toggles = toggles
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Features(Dictionary<string, bool> toggles, CancellationToken ct)
    {
        var instituteId = await _scope.GetCurrentInstituteIdAsync(User, ct);
        if (instituteId is null)
            return Forbid();

        var institute = await _db.Institutes
            .FirstOrDefaultAsync(i => i.Id == instituteId.Value, ct);

        if (institute is null)
            return NotFound();

        foreach (var key in PlatformFeatureKeys.AllToggles)
        {
            if (!LicenseTierFeatureMap.IsTierAllowed(key, institute.LicenseTier))
                continue; // skip toggles the tier doesn't allow

            var isEnabled = toggles.TryGetValue(key, out var val) && val;
            await _config.SetFeatureAsync(key, isEnabled, User, instituteId, ct);
        }

        TempData["Success"] = "Feature settings updated successfully.";
        return RedirectToAction(nameof(Features));
    }

    /// <summary>
    /// Converts a feature key like "feature.codingExam.enabled" to "Coding Exam".
    /// </summary>
    private static string FormatDisplayName(string key)
    {
        // Remove prefix "feature." and suffix ".enabled"/".required"/etc.
        var parts = key.Split('.');
        if (parts.Length < 2) return key;

        var middle = parts.Length >= 3 ? parts[1] : parts[^1];

        // Convert camelCase to Title Case
        var result = System.Text.RegularExpressions.Regex.Replace(
            middle, "([a-z])([A-Z])", "$1 $2");
        return char.ToUpperInvariant(result[0]) + result[1..];
    }
}
