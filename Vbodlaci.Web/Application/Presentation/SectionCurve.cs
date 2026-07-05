namespace Vbodlaci.Web.Application.Presentation;

/// <summary>
/// Parameters for the curved SVG section transition partial (_Curve.cshtml).
/// Fill is the color of the next section (the "hill" rising into the previous one);
/// Background fills the strip behind the curve and must match the previous section.
/// Values may be CSS variables, e.g. "var(--cream)".
/// </summary>
public sealed record SectionCurve(string Fill, string? Background = null, bool Hero = false);
