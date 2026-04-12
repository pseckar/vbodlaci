using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public sealed class CourseQueryFilter
{
    public CourseType? Type { get; set; }

    public bool IncludeCanceled { get; set; }

    public int? Take { get; set; }
}
