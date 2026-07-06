using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

public sealed class IndexModel(ICourseService courseService) : PageModel
{
    public IReadOnlyList<CourseListItem> Courses { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Courses = await courseService.GetAdminCoursesAsync(cancellationToken);
    }
}
