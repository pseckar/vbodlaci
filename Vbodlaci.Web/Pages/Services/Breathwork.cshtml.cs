using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Pages.Services;

public sealed class BreathworkModel(ICourseService courseService) : PageModel
{
    public IReadOnlyList<CourseListItem> Courses { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Courses = await courseService.GetPublicCoursesAsync(new CourseQueryFilter
        {
            Type = CourseType.Breathwork
        }, cancellationToken);
    }
}
