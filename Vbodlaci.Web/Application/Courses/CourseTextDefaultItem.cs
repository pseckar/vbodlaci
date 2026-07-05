using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public sealed class CourseTextDefaultItem
{
    public CourseType Type { get; init; }

    public CourseTextField Field { get; init; }

    public string Text { get; init; } = string.Empty;
}
