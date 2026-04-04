using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Security;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

[Authorize(Roles = AppRoles.Admin)]
public class EditModel(ICourseRepository courseRepository) : PageModel
{
    [BindProperty]
    public CourseInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var course = await courseRepository.GetByIdAsync(id, HttpContext.RequestAborted);
        if (course is null)
        {
            return NotFound();
        }

        Input = new CourseInputModel
        {
            Title = course.Title,
            Slug = course.Slug,
            ShortDescription = course.ShortDescription,
            Description = course.Description,
            StartDate = course.StartDate.ToDateTime(TimeOnly.MinValue),
            Capacity = course.Capacity,
            IsPublished = course.IsPublished
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var course = await courseRepository.GetByIdAsync(id, HttpContext.RequestAborted);
        if (course is null)
        {
            return NotFound();
        }

        var slugSource = string.IsNullOrWhiteSpace(Input.Slug) ? Input.Title : Input.Slug;
        var slug = SlugGenerator.Create(slugSource ?? string.Empty);
        if (string.IsNullOrWhiteSpace(slug))
        {
            ModelState.AddModelError(nameof(Input.Slug), "Slug nebylo možné vytvořit.");
            return Page();
        }

        if (await courseRepository.SlugExistsAsync(slug, id, HttpContext.RequestAborted))
        {
            ModelState.AddModelError(nameof(Input.Slug), "Slug už existuje, zvol jiný.");
            return Page();
        }

        course.Title = Input.Title.Trim();
        course.Slug = slug;
        course.ShortDescription = Input.ShortDescription.Trim();
        course.Description = Input.Description.Trim();
        course.StartDate = DateOnly.FromDateTime(Input.StartDate);
        course.Capacity = Input.Capacity;
        course.IsPublished = Input.IsPublished;
        course.UpdatedAt = DateTimeOffset.UtcNow;

        await courseRepository.UpdateAsync(course, HttpContext.RequestAborted);
        TempData["StatusMessage"] = "Kurz byl upraven.";

        return RedirectToPage("/Courses/Index", new { area = "Admin" });
    }
}
