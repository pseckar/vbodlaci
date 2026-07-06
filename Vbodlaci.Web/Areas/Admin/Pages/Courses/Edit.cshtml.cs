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

    public CourseStatus CurrentStatus { get; private set; } = CourseStatus.Draft;

    public string FacebookPostText { get; private set; } = string.Empty;

    public string CurrentImageUrl { get; private set; } = string.Empty;

    public string PublicUrl { get; private set; } = string.Empty;

    public CourseListItem PreviewItem { get; private set; } = new();

    public bool IsReadOnly => CurrentStatus == CourseStatus.Canceled;

    public bool IsDraft => CurrentStatus == CourseStatus.Draft;

    public bool IsPublished => CurrentStatus == CourseStatus.Published;

    public string StatusLabel => CurrentStatus switch
    {
        CourseStatus.Draft => "Draft",
        CourseStatus.Published => "Publikovaný",
        CourseStatus.Canceled => "Zrušen",
        _ => CurrentStatus.ToString()
    };

    public string CourseTypeLabel => Input.Type == CourseType.Breathwork ? "Breathwork" : "Koně";

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await courseService.GetCourseByIdAsync(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        PopulateFromDetail(detail);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await courseService.GetCourseByIdAsync(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        // populate page context only — replacing Input here would discard the posted edits
        PopulateContext(detail);

        if (detail.Status == CourseStatus.Canceled)
        {
            TempData["FlashMessage"] = "Zrušený kurz je dostupný pouze pro čtení.";
            TempData["FlashType"] = "error";
            return RedirectToPage(new { id });
        }

        Input.Status = detail.Status;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await courseService.UpdateAsync(id, Input, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";

        if (!result.Succeeded)
        {
            var reloaded = await courseService.GetCourseByIdAsync(id, cancellationToken);
            if (reloaded is not null)
            {
                PopulateFromDetail(reloaded);
            }

            return Page();
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostPublishAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await courseService.ChangeStatusAsync(id, CourseStatus.Published, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await courseService.ChangeStatusAsync(id, CourseStatus.Canceled, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await courseService.SoftDeleteAsync(id, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";

        return result.Succeeded
            ? RedirectToPage("/Courses/Index")
            : RedirectToPage(new { id });
    }

    private void PopulateContext(CourseDetailViewModel detail)
    {
        CourseId = detail.Id;
        CurrentStatus = detail.Status;
        FacebookPostText = detail.FacebookPostText;
        CurrentImageUrl = detail.ImageUrl;
        PublicUrl = Url.Page("/Courses/Detail", pageHandler: null,
            values: new { area = "", slug = detail.Slug }, protocol: Request.Scheme) ?? string.Empty;
        PreviewItem = new CourseListItem
        {
            Id = detail.Id,
            Type = detail.Type,
            Status = detail.Status,
            Title = detail.Title,
            Slug = detail.Slug,
            CourseDate = detail.CourseDate,
            TimeText = detail.TimeText,
            CityOrArea = detail.CityOrArea,
            PriceText = detail.PriceText,
            ShortDescription = detail.ShortDescription,
            ThumbnailImageUrl = detail.ThumbnailImageUrl
        };
    }

    private void PopulateFromDetail(CourseDetailViewModel detail)
    {
        PopulateContext(detail);
        Input = new CourseEditModel
        {
            Id = detail.Id,
            Type = detail.Type,
            Status = detail.Status,
            Title = detail.Title,
            CourseDate = detail.CourseDate,
            TimeText = detail.TimeText,
            CityOrArea = detail.CityOrArea,
            PriceText = detail.PriceText,
            CapacityInfo = detail.CapacityInfo,
            ShortDescription = detail.ShortDescription,
            FullDescription = detail.FullDescription,
            IsFullDescriptionVisible = detail.IsFullDescriptionVisible,
            WhatToExpect = detail.WhatToExpect,
            IsWhatToExpectVisible = detail.IsWhatToExpectVisible
        };
    }
}
