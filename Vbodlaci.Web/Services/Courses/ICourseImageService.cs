using Microsoft.AspNetCore.Http;
using Vbodlaci.Web.Application.Common;

namespace Vbodlaci.Web.Services.Courses;

public interface ICourseImageService
{
    Task<(ServiceResult Result, CourseImageResult? Image)> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);

    void DeleteCourseImages(string imagePath, string thumbnailPath);
}
