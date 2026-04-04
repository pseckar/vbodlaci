using System.ComponentModel.DataAnnotations;

namespace Vbodlaci.Web.Application.Courses;

public sealed class CourseInputModel
{
    [Required(ErrorMessage = "Název kurzu je povinný.")]
    [StringLength(160, ErrorMessage = "Název může mít maximálně 160 znaků.")]
    [Display(Name = "Název kurzu")]
    public string Title { get; set; } = string.Empty;

    [StringLength(180, ErrorMessage = "Slug může mít maximálně 180 znaků.")]
    [Display(Name = "Slug URL (volitelné)")]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Krátký popis je povinný.")]
    [StringLength(400, ErrorMessage = "Krátký popis může mít maximálně 400 znaků.")]
    [Display(Name = "Krátký popis")]
    public string ShortDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Detailní popis je povinný.")]
    [StringLength(6000, ErrorMessage = "Detailní popis může mít maximálně 6000 znaků.")]
    [Display(Name = "Detailní popis")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Datum začátku je povinné.")]
    [DataType(DataType.Date)]
    [Display(Name = "Datum začátku")]
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(14);

    [Range(1, 2000, ErrorMessage = "Kapacita musí být mezi 1 a 2000.")]
    [Display(Name = "Kapacita")]
    public int Capacity { get; set; } = 12;

    [Display(Name = "Publikovat kurz")]
    public bool IsPublished { get; set; }
}
