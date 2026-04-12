using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public sealed class CourseListItem
{
    public Guid Id { get; init; }

    public CourseType Type { get; init; }

    public CourseStatus Status { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public DateTimeOffset StartDateTime { get; init; }

    public string CityOrArea { get; init; } = string.Empty;

    public string PriceText { get; init; } = string.Empty;

    public string ShortDescription { get; init; } = string.Empty;
}
