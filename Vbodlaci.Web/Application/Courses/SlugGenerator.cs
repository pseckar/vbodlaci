using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Vbodlaci.Web.Application.Courses;

public static class SlugGenerator
{
    private static readonly Regex DuplicateDashRegex = new("-{2,}", RegexOptions.Compiled);
    private static readonly Regex InvalidCharRegex = new("[^a-z0-9-]", RegexOptions.Compiled);

    public static string Generate(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return "kurz";
        }

        var normalized = source.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var candidate = char.ToLowerInvariant(ch);
            builder.Append(char.IsLetterOrDigit(candidate) ? candidate : '-');
        }

        var slug = builder.ToString().Normalize(NormalizationForm.FormC);
        slug = InvalidCharRegex.Replace(slug, string.Empty);
        slug = DuplicateDashRegex.Replace(slug, "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "kurz" : slug;
    }
}
