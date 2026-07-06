using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Areas.Admin.Pages.Courses;

public sealed class CreateModel(ICourseService courseService) : PageModel
{
    [BindProperty]
    public CourseEditModel Input { get; set; } = new();

    [BindProperty]
    public CourseType DefaultType { get; set; }

    [BindProperty]
    public CourseTextField DefaultField { get; set; }

    // nullable: posted only by the default-text dialog form; a non-nullable
    // string would add an implicit required error to every course submit
    [BindProperty]
    public string? DefaultText { get; set; }

    public IReadOnlyList<CourseTextDefaultItem> DefaultTexts { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await PopulateDefaultsAsync(cancellationToken);
        Input.Type = CourseType.Breathwork;
        Input.Status = CourseStatus.Draft;
        Input.CourseDate = DateOnly.FromDateTime(DateTime.Now.AddDays(14));
        Input.TimeText = "18:00-21:00";
        Input.PriceText = "0 Kč";
        Input.IsFullDescriptionVisible = true;
        Input.IsWhatToExpectVisible = true;
        ApplyTextDefaults(Input.Type);
    }

    public Task<IActionResult> OnPostPublishAsync(CancellationToken cancellationToken)
    {
        Input.Status = CourseStatus.Published;
        return CreateCourseAsync(cancellationToken);
    }

    public Task<IActionResult> OnPostDraftAsync(CancellationToken cancellationToken)
    {
        Input.Status = CourseStatus.Draft;
        return CreateCourseAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostDefaultTextAsync(CancellationToken cancellationToken)
    {
        ModelState.Clear();
        var result = await courseService.UpdateTextDefaultAsync(DefaultType, DefaultField, DefaultText ?? string.Empty, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return RedirectToPage();
    }

    private async Task<IActionResult> CreateCourseAsync(CancellationToken cancellationToken)
    {
        await PopulateDefaultsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (result, id) = await courseService.CreateAsync(Input, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";

        if (result.Succeeded && id.HasValue)
        {
            return RedirectToPage("/Courses/Index");
        }

        return Page();
    }

    private async Task PopulateDefaultsAsync(CancellationToken cancellationToken)
    {
        DefaultTexts = await courseService.GetTextDefaultsAsync(cancellationToken);
    }

    private void ApplyTextDefaults(CourseType type)
    {
        Input.ShortDescription = GetDefault(type, CourseTextField.ShortDescription);
        Input.FullDescription = GetDefault(type, CourseTextField.FullDescription);
        Input.WhatToExpect = GetDefault(type, CourseTextField.WhatToExpect);
    }

    private string GetDefault(CourseType type, CourseTextField field)
    {
        return DefaultTexts.FirstOrDefault(item => item.Type == type && item.Field == field)?.Text
               ?? "This is placeholder for default text";
    }
}
