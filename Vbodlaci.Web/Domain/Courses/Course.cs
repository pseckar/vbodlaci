namespace Vbodlaci.Web.Domain.Courses;

public sealed class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public int Capacity { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
