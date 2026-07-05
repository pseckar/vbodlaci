using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public sealed class CourseDetailViewModel
{
    public Guid Id { get; init; }

    public CourseType Type { get; init; }

    public CourseStatus Status { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public DateOnly CourseDate { get; init; }

    public string TimeText { get; init; } = string.Empty;

    public string CityOrArea { get; init; } = string.Empty;

    public string PriceText { get; init; } = string.Empty;

    public int? CapacityInfo { get; init; }

    public string ShortDescription { get; init; } = string.Empty;

    public string FullDescription { get; init; } = string.Empty;

    public bool IsFullDescriptionVisible { get; init; }

    public string WhatToExpect { get; init; } = string.Empty;

    public bool IsWhatToExpectVisible { get; init; }

    public string ImageUrl { get; init; } = string.Empty;

    public string ThumbnailImageUrl { get; init; } = string.Empty;

    public DateTimeOffset? PublishedAt { get; init; }

    public string FacebookPostText { get; init; } = string.Empty;
}
