namespace Vbodlaci.Web.Application.Presentation;

/// <summary>
/// Czech month abbreviations for the course-card date block. Hardcoded so the
/// rendered labels do not depend on host ICU/CLDR data.
/// </summary>
public static class CzechMonths
{
    private static readonly string[] Abbreviations =
    [
        "led", "úno", "bře", "dub", "kvě", "čvn", "čvc", "srp", "zář", "říj", "lis", "pro"
    ];

    public static string Abbreviation(int month)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
        return Abbreviations[month - 1];
    }

    /// <summary>
    /// Month label for the date block; adds a shortened year when the course
    /// runs in a different year than the current one (e.g. "srp ’27").
    /// </summary>
    public static string MonthLabel(DateOnly date, int currentYear)
    {
        var abbreviation = Abbreviation(date.Month);
        return date.Year == currentYear
            ? abbreviation
            : $"{abbreviation} ’{date.Year % 100:00}";
    }
}
