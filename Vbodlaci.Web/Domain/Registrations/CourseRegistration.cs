using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Domain.Registrations;

public sealed class CourseRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public bool TermsConsent { get; set; }

    public string ClientIp { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
