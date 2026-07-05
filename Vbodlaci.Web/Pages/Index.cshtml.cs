using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Configuration;
using Vbodlaci.Web.Application.Courses;

namespace Vbodlaci.Web.Pages;

public sealed class IndexModel(ICourseService courseService, IOptions<SiteOptions> siteOptions) : PageModel
{
    public IReadOnlyList<CourseListItem> Courses { get; private set; } = [];

    public string SelectedType { get; private set; } = "all";

    public SiteOptions Site => siteOptions.Value;

    public async Task OnGetAsync([FromQuery(Name = "typ")] string? typ, CancellationToken cancellationToken)
    {
        var normalized = (typ ?? "all").Trim().ToLowerInvariant();
        var filter = new CourseQueryFilter();

        if (normalized == "breathwork")
        {
            SelectedType = "breathwork";
        }
        else if (normalized == "kone")
        {
            SelectedType = "kone";
        }
        else
        {
            SelectedType = "all";
        }

        Courses = await courseService.GetPublicCoursesAsync(filter, cancellationToken);
    }
}

