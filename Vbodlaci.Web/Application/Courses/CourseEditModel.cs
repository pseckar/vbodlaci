using System.ComponentModel.DataAnnotations;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public sealed class CourseEditModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Vyber typ kurzu.")]
    public CourseType Type { get; set; }

    [Required(ErrorMessage = "Název je povinný.")]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Termín je povinný.")]
    public DateTimeOffset StartDateTime { get; set; }

    public DateTimeOffset? EndDateTime { get; set; }

    [Required(ErrorMessage = "Místo je povinné.")]
    [StringLength(160)]
    public string CityOrArea { get; set; } = string.Empty;

    [StringLength(180)]
    public string VenueText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cena je povinná.")]
    [StringLength(120)]
    public string PriceText { get; set; } = string.Empty;

    [Range(1, 999)]
    public int? CapacityInfo { get; set; }

    public DateTimeOffset? RegistrationDeadline { get; set; }

    [Required(ErrorMessage = "Krátký popis je povinný.")]
    [StringLength(400)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Detailní popis je povinný.")]
    [StringLength(8000)]
    public string FullDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vyplň část Co tě čeká.")]
    [StringLength(2000)]
    public string WhatToExpect { get; set; } = string.Empty;

    [Required]
    public CourseStatus Status { get; set; }
}
