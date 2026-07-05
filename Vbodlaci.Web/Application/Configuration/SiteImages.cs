namespace Vbodlaci.Web.Application.Configuration;

/// <summary>
/// Central image-slot constants for public pages (SPECIFICATION.md §25.1).
/// All slots currently point to the shared placeholder photo; when final
/// photography is delivered, drop the files into wwwroot/images and update
/// the paths here (and remove the placeholder crop/filter CSS modifiers).
/// </summary>
public static class SiteImages
{
    public const string HeroHome = "/images/title-image.jpeg";
    public const string HeroBreathwork = "/images/title-image.jpeg";
    public const string HeroHorses = "/images/title-image.jpeg";
    public const string HeroVeterinary = "/images/title-image.jpeg";
    public const string ServiceCardBreathwork = "/images/title-image.jpeg";
    public const string ServiceCardHorses = "/images/title-image.jpeg";
    public const string ServiceCardVeterinary = "/images/title-image.jpeg";
    public const string AboutPortrait = "/images/title-image.jpeg";
}
