using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Application.Courses;

namespace Vbodlaci.Web.Services.Courses;

public sealed class CourseImageService(IWebHostEnvironment environment) : ICourseImageService
{
    private const int ThumbnailWidth = 600;
    private const int MinRecommendedWidth = 1200;
    private const int MinRecommendedHeight = 800;
    private const long MaxUploadBytes = 8 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public async Task<(ServiceResult Result, CourseImageResult? Image)> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return (ServiceResult.Failure("Soubor s fotkou je prázdný."), null);
        }

        if (file.Length > MaxUploadBytes)
        {
            return (ServiceResult.Failure("Fotka je příliš velká. Nahraj prosím obrázek do 8 MB."), null);
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return (ServiceResult.Failure("Nahraj prosím fotku ve formátu JPG, PNG nebo WebP."), null);
        }

        Image image;
        try
        {
            await using var readStream = file.OpenReadStream();
            image = await Image.LoadAsync(readStream, cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            return (ServiceResult.Failure("Soubor se nepodařilo načíst jako obrázek."), null);
        }
        catch (InvalidImageContentException)
        {
            return (ServiceResult.Failure("Soubor se nepodařilo načíst jako obrázek."), null);
        }

        using (image)
        {
            var warning = image.Width < MinRecommendedWidth || image.Height < MinRecommendedHeight
                ? "Nahraná fotka je menší než doporučených 1200 x 800 px. Na webu může působit méně ostře."
                : null;

            var uploadsRoot = Path.Combine(environment.WebRootPath, "uploads", "courses");
            Directory.CreateDirectory(uploadsRoot);

            var fileStem = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            var imageFileName = $"{fileStem}.jpg";
            var thumbFileName = $"{fileStem}-thumb.jpg";
            var imagePath = Path.Combine(uploadsRoot, imageFileName);
            var thumbnailPath = Path.Combine(uploadsRoot, thumbFileName);

            var encoder = new JpegEncoder { Quality = 86 };
            await image.SaveAsJpegAsync(imagePath, encoder, cancellationToken);

            using var thumbnail = image.Clone(context =>
            {
                if (image.Width > ThumbnailWidth)
                {
                    context.Resize(new ResizeOptions
                    {
                        Size = new Size(ThumbnailWidth, 0),
                        Mode = ResizeMode.Max
                    });
                }
            });
            await thumbnail.SaveAsJpegAsync(thumbnailPath, encoder, cancellationToken);

            return (ServiceResult.Success(warning ?? "Fotka byla nahrána."), new CourseImageResult
            {
                ImagePath = $"/uploads/courses/{imageFileName}",
                ThumbnailPath = $"/uploads/courses/{thumbFileName}",
                Warning = warning
            });
        }
    }

    public void DeleteCourseImages(string imagePath, string thumbnailPath)
    {
        DeleteIfCourseUpload(imagePath);
        if (!string.Equals(imagePath, thumbnailPath, StringComparison.OrdinalIgnoreCase))
        {
            DeleteIfCourseUpload(thumbnailPath);
        }
    }

    private void DeleteIfCourseUpload(string webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath) ||
            string.Equals(webPath, CourseImageDefaults.DefaultImagePath, StringComparison.OrdinalIgnoreCase) ||
            !webPath.StartsWith("/uploads/courses/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(environment.WebRootPath, relativePath));
        var uploadRoot = Path.GetFullPath(Path.Combine(environment.WebRootPath, "uploads", "courses"));

        if (!fullPath.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            return;
        }

        File.Delete(fullPath);
    }
}
