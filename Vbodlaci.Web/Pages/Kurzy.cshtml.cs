using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Pages;

public class KurzyModel(ICourseRepository courseRepository) : PageModel
{
    public IReadOnlyList<Course> Courses { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Courses = await courseRepository.GetPublishedAsync(HttpContext.RequestAborted);
    }
}
