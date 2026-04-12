using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

public sealed class IndexModel(ICourseService courseService) : PageModel
{
    public IReadOnlyList<CourseListItem> Courses { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Courses = await courseService.GetAdminCoursesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostStatusAsync(Guid id, CourseStatus status, CancellationToken cancellationToken)
    {
        var result = await courseService.ChangeStatusAsync(id, status, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await courseService.SoftDeleteAsync(id, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return RedirectToPage();
    }
}
