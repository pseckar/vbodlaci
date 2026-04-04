namespace Vbodlaci.Web.Application.Security;

public sealed class AdminSeedOptions
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
