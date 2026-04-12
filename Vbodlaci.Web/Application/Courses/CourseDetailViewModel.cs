using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public sealed class CourseDetailViewModel
{
    public Guid Id { get; init; }

    public CourseType Type { get; init; }

    public CourseStatus Status { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public DateTimeOffset StartDateTime { get; init; }

    public DateTimeOffset? EndDateTime { get; init; }

    public string CityOrArea { get; init; } = string.Empty;

    public string VenueText { get; init; } = string.Empty;

    public string PriceText { get; init; } = string.Empty;

    public int? CapacityInfo { get; init; }

    public DateTimeOffset? RegistrationDeadline { get; init; }

    public string ShortDescription { get; init; } = string.Empty;

    public string FullDescription { get; init; } = string.Empty;

    public string WhatToExpect { get; init; } = string.Empty;

    public DateTimeOffset? PublishedAt { get; init; }

    public string FacebookPostText { get; init; } = string.Empty;
}
