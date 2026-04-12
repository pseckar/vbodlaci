using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

public sealed class EditModel(ICourseService courseService) : PageModel
{
    [BindProperty]
    public CourseEditModel Input { get; set; } = new();

    public Guid CourseId { get; private set; }

    public string FacebookPostText { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await courseService.GetCourseByIdAsync(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        CourseId = detail.Id;
        FacebookPostText = detail.FacebookPostText;
        Input = new CourseEditModel
        {
            Id = detail.Id,
            Type = detail.Type,
            Status = detail.Status,
            Title = detail.Title,
            StartDateTime = detail.StartDateTime.ToLocalTime(),
            EndDateTime = detail.EndDateTime?.ToLocalTime(),
            CityOrArea = detail.CityOrArea,
            VenueText = detail.VenueText,
            PriceText = detail.PriceText,
            CapacityInfo = detail.CapacityInfo,
            RegistrationDeadline = detail.RegistrationDeadline?.ToLocalTime(),
            ShortDescription = detail.ShortDescription,
            FullDescription = detail.FullDescription,
            WhatToExpect = detail.WhatToExpect
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            CourseId = id;
            return Page();
        }

        var result = await courseService.UpdateAsync(id, Input, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";

        if (!result.Succeeded)
        {
            CourseId = id;
            return Page();
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostStatusAsync(Guid id, CourseStatus status, CancellationToken cancellationToken)
    {
        var result = await courseService.ChangeStatusAsync(id, status, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return RedirectToPage(new { id });
    }
}
