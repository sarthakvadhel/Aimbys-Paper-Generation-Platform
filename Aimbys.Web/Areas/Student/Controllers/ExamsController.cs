using System.Security.Claims;
using Aimbys.Application.Exams;
using Aimbys.Domain.Enums;
using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = Roles.Student)]
public class ExamsController : Controller
{
    private readonly IExamSecurityService _securityService;

    public ExamsController(IExamSecurityService securityService)
    {
        _securityService = securityService;
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _securityService.RecordHeartbeatAsync(request.AttemptId, userId);
        return success ? Ok() : BadRequest();
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _securityService.RecordEventAsync(
            request.AttemptId, request.EventType, request.DetailsJson, userId);
        return success ? Ok() : BadRequest();
    }
}

public class HeartbeatRequest
{
    public Guid AttemptId { get; set; }
}

public class TrackEventRequest
{
    public Guid AttemptId { get; set; }
    public ExamEventType EventType { get; set; }
    public string? DetailsJson { get; set; }
}
