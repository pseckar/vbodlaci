using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Pages.Sluzby;

public sealed class KoneModel(ICourseService courseService) : PageModel
{
    public IReadOnlyList<CourseListItem> Courses { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Courses = await courseService.GetPublicCoursesAsync(new CourseQueryFilter
        {
            Type = CourseType.Kone,
            Take = 12
        }, cancellationToken);
    }
}
