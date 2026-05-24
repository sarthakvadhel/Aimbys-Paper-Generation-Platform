using Aimbys.Application.Questions;
using Aimbys.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aimbys.Web.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = Roles.Teacher)]
public class QuestionsController : Controller
{
    private readonly IQuestionLifecycleService _lifecycle;

    public QuestionsController(IQuestionLifecycleService lifecycle)
    {
        _lifecycle = lifecycle;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var result = await _lifecycle.SubmitForReviewAsync(id, User, ct);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index", "Home");
        }

        TempData["Success"] = "Question submitted for review.";
        return RedirectToAction("Index", "Home");
    }
}
