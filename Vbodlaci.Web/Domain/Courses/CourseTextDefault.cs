namespace Vbodlaci.Web.Domain.Courses;

public sealed class CourseTextDefault
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public CourseType Type { get; set; }

    public CourseTextField Field { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }
}
