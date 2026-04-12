using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Courses;

public interface ICourseService
{
    Task<IReadOnlyList<CourseListItem>> GetPublicCoursesAsync(CourseQueryFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseListItem>> GetAdminCoursesAsync(CancellationToken cancellationToken = default);

    Task<CourseDetailViewModel?> GetPublicCourseBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<CourseDetailViewModel?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(ServiceResult Result, Guid? Id)> CreateAsync(CourseEditModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(Guid id, CourseEditModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResult> ChangeStatusAsync(Guid id, CourseStatus status, CancellationToken cancellationToken = default);
}
