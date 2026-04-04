using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

[Authorize(Roles = AppRoles.Admin)]
public class DeleteModel(ICourseRepository courseRepository) : PageModel
{
    public Course? Course { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Course = await courseRepository.GetByIdAsync(id, HttpContext.RequestAborted);
        if (Course is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var course = await courseRepository.GetByIdAsync(id, HttpContext.RequestAborted);
        if (course is null)
        {
            return NotFound();
        }

        await courseRepository.DeleteAsync(course, HttpContext.RequestAborted);
        TempData["StatusMessage"] = "Kurz byl smazán.";

        return RedirectToPage("/Courses/Index", new { area = "Admin" });
    }
}
