using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Admin.Controllers;

/// <summary>
/// Placeholder Admin-area controller. Exists to prove that authorization
/// works end-to-end before any real admin features land.
/// </summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
