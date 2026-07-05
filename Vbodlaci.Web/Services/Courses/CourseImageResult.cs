namespace Vbodlaci.Web.Services.Courses;

public sealed class CourseImageResult
{
    public string ImagePath { get; init; } = string.Empty;

    public string ThumbnailPath { get; init; } = string.Empty;

    public string? Warning { get; init; }
}
