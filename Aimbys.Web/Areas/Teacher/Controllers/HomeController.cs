using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Teacher.Controllers;

/// <summary>
/// Landing surface for the Teacher / Examiner role. Reached via
/// <c>/Teacher</c> after sign-in. Real teacher tooling (paper
/// generation, evaluation, moderation queues) lands in later chunks;
/// this page exists so Chunk 14's post-login redirect has a target.
/// </summary>
[Area("Teacher")]
[Authorize(Roles = Roles.Teacher)]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
