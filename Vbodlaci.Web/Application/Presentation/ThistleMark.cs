namespace Vbodlaci.Web.Application.Presentation;

/// <summary>
/// Parameters for the shared thistle SVG mark partial (_Thistle.cshtml).
/// The mark itself is defined once in _ThistleDefs.cshtml and reused via svg use.
/// </summary>
public sealed record ThistleMark(string CssClass, string? Style = null);
