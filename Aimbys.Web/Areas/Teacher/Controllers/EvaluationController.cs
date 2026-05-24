using Aimbys.Application.Exams;
using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = Roles.Teacher)]
public class EvaluationController : Controller
{
    private readonly IExamSecurityService _securityService;

    public EvaluationController(IExamSecurityService securityService)
    {
        _securityService = securityService;
    }

    [HttpGet]
    public async Task<IActionResult> EventTimeline(Guid attemptId)
    {
        var timeline = await _securityService.GetTimelineAsync(attemptId);
        return View(timeline);
    }
}
