using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public interface ICourseRepository
{
    Task<IReadOnlyList<Course>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, Guid? excludingCourseId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Course course, CancellationToken cancellationToken = default);

    Task UpdateAsync(Course course, CancellationToken cancellationToken = default);

    Task DeleteAsync(Course course, CancellationToken cancellationToken = default);
}
