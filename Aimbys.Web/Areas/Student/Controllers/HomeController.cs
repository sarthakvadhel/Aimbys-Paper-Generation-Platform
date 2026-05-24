using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Student.Controllers;

/// <summary>
/// Landing surface for the Student / Candidate role. Reached via
/// <c>/Student</c> after sign-in. Real student tooling (exam
/// access, results, certificates) lands in later chunks; this page
/// exists so Chunk 14's post-login redirect has a target.
/// </summary>
[Area("Student")]
[Authorize(Roles = Roles.Student)]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
