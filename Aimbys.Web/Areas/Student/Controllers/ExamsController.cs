using Aimbys.Application.Coding;
using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Student.Controllers;

/// <summary>
/// Exam-taking endpoints for students. Coding question support allows
/// students to run sample test cases during the exam and submit their
/// final code for full evaluation.
/// </summary>
[Area("Student")]
[Authorize(Roles = Roles.Student)]
public class ExamsController : Controller
{
    private readonly ICodeExecutionService _execution;

    public ExamsController(ICodeExecutionService execution)
    {
        _execution = execution;
    }

    /// <summary>
    /// POST: /Student/Exams/RunSample — student runs sample test cases
    /// during an exam attempt.
    /// JSON endpoint; CSRF protection via header.
    /// </summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RunSample([FromBody] CodeRunRequest request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.SourceCode))
        {
            return BadRequest(new { error = "Source code is required." });
        }

        var result = await _execution.RunSampleAsync(request with { SampleOnly = true }, ct);
        return Json(result);
    }

    /// <summary>
    /// POST: /Student/Exams/SubmitCode — student submits code for full
    /// execution (including hidden test cases).
    /// JSON endpoint; CSRF protection via header.
    /// </summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SubmitCode([FromBody] CodeRunRequest request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.SourceCode))
        {
            return BadRequest(new { error = "Source code is required." });
        }

        var result = await _execution.RunFullAsync(request with { SampleOnly = false }, ct);
        return Json(result);
    }
}
