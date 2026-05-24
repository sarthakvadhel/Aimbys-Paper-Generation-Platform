using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.SuperAdmin.Controllers;

/// <summary>
/// Landing surface for platform-support staff. Reached via
/// <c>/SuperAdmin</c> after a SuperAdmin signs in &mdash; either via
/// <see cref="Aimbys.Web.Identity.RoleHomeRedirector"/> or by a deep
/// link. Real platform tooling (institute approvals, audit viewer,
/// system health) lands in later chunks; this page exists so the
/// post-login redirect contract is honoured today.
/// </summary>
[Area("SuperAdmin")]
[Authorize(Roles = Roles.SuperAdmin)]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
