using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

[Authorize(Roles = AppRoles.Admin)]
public class CreateModel(ICourseRepository courseRepository) : PageModel
{
    [BindProperty]
    public CourseInputModel Input { get; set; } = new();

    public void OnGet()
    {
        Input = new CourseInputModel
        {
            StartDate = DateTime.Today.AddDays(14),
            Capacity = 12,
            IsPublished = false
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var slugSource = string.IsNullOrWhiteSpace(Input.Slug) ? Input.Title : Input.Slug;
        var slug = SlugGenerator.Create(slugSource ?? string.Empty);
        if (string.IsNullOrWhiteSpace(slug))
        {
            ModelState.AddModelError(nameof(Input.Slug), "Slug nebylo možné vytvořit.");
            return Page();
        }

        if (await courseRepository.SlugExistsAsync(slug, cancellationToken: HttpContext.RequestAborted))
        {
            ModelState.AddModelError(nameof(Input.Slug), "Slug už existuje, zvol jiný.");
            return Page();
        }

        var now = DateTimeOffset.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = Input.Title.Trim(),
            Slug = slug,
            ShortDescription = Input.ShortDescription.Trim(),
            Description = Input.Description.Trim(),
            StartDate = DateOnly.FromDateTime(Input.StartDate),
            Capacity = Input.Capacity,
            IsPublished = Input.IsPublished,
            CreatedAt = now,
            UpdatedAt = now
        };

        await courseRepository.AddAsync(course, HttpContext.RequestAborted);
        TempData["StatusMessage"] = "Kurz byl vytvořen.";

        return RedirectToPage("/Courses/Index", new { area = "Admin" });
    }
}
