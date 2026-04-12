using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

public sealed class CreateModel(ICourseService courseService) : PageModel
{
    [BindProperty]
    public CourseEditModel Input { get; set; } = new();

    public void OnGet()
    {
        Input.Type = CourseType.Breathwork;
        Input.Status = CourseStatus.Draft;
        Input.StartDateTime = DateTimeOffset.Now.AddDays(14);
        Input.PriceText = "0 Kč";
    }

    public Task<IActionResult> OnPostPublishAsync(CancellationToken cancellationToken)
    {
        Input.Status = CourseStatus.Published;
        return CreateCourseAsync(cancellationToken);
    }

    public Task<IActionResult> OnPostDraftAsync(CancellationToken cancellationToken)
    {
        Input.Status = CourseStatus.Draft;
        return CreateCourseAsync(cancellationToken);
    }

    private async Task<IActionResult> CreateCourseAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (result, id) = await courseService.CreateAsync(Input, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";

        if (result.Succeeded && id.HasValue)
        {
            return RedirectToPage("/Courses/Index");
        }

        return Page();
    }
}
