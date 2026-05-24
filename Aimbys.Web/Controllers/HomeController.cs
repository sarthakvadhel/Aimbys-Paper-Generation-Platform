using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Aimbys.Web.Models;

namespace Aimbys.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Landing page shown by
    /// <see cref="Aimbys.Web.Middleware.SubscriptionEnforcementMiddleware"/>
    /// when the institute's subscription is in a state that blocks
    /// access (Suspended / Expired / GracePeriod past expiry).
    /// Allows anonymous so the user can read the page after the
    /// authenticated session is interrupted by the redirect.
    /// </summary>
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult SubscriptionSuspended()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
