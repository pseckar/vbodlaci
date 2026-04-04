using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

[Authorize(Roles = AppRoles.Admin)]
public class IndexModel(ICourseRepository courseRepository) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<Course> Courses { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Courses = await courseRepository.GetAllAsync(HttpContext.RequestAborted);
    }
}
