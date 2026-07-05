using Vbodlaci.Web.Domain.Registrations;

namespace Vbodlaci.Web.Domain.Courses;

public sealed class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public CourseType Type { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public DateOnly CourseDate { get; set; }

    public string TimeText { get; set; } = string.Empty;

    public string CityOrArea { get; set; } = string.Empty;

    public string PriceText { get; set; } = string.Empty;

    public int? CapacityInfo { get; set; }

    public string ShortDescription { get; set; } = string.Empty;

    public string FullDescription { get; set; } = string.Empty;

    public bool IsFullDescriptionVisible { get; set; } = true;

    public string WhatToExpect { get; set; } = string.Empty;

    public bool IsWhatToExpectVisible { get; set; } = true;

    public string ImagePath { get; set; } = string.Empty;

    public string ThumbnailPath { get; set; } = string.Empty;

    public DateTimeOffset? PublishedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<CourseRegistration> Registrations { get; set; } = new List<CourseRegistration>();
}
