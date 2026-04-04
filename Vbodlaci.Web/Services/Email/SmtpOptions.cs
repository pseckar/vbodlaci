namespace Vbodlaci.Web.Services.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Email:Smtp";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    public string From { get; set; } = "noreply@example.local";
}
