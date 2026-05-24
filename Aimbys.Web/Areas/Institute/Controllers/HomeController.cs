using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Institute.Controllers;

/// <summary>
/// Landing surface for an Institute Administrator (the tenant owner).
/// Reached via <c>/Institute</c> after sign-in. Tenant-management
/// features (departments, subjects, class batches, teacher invites)
/// land in later chunks; this stub exists so the post-login redirect
/// contract from Chunk 14 has a target.
/// </summary>
[Area("Institute")]
[Authorize(Roles = Roles.InstituteAdmin)]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
