using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Admin.Controllers;

/// <summary>
/// Placeholder Admin-area controller from the PR #5 baseline. Re-pointed at
/// <see cref="Roles.SuperAdmin"/> by Chunk 8 so the build stays green after
/// the legacy <c>Admin</c> / <c>User</c> roles are removed. Chunk 13
/// replaces this entirely with four role-specific MVC Areas
/// (<c>SuperAdmin</c>, <c>Institute</c>, <c>Teacher</c>, <c>Student</c>).
/// </summary>
[Area("Admin")]
[Authorize(Roles = Roles.SuperAdmin)]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
