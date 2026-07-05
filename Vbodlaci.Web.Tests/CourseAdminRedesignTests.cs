using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Services.Courses;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class CourseAdminRedesignTests
{
    [Fact]
    public async Task PublicCourses_ReturnAllFuturePublishedCourses_OrderedByCourseDate()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ICourseService>();
        var now = DateTimeOffset.UtcNow;

        for (var index = 0; index < 15; index++)
        {
            db.Courses.Add(CreateCourse($"kurz-{index}", DateOnly.FromDateTime(DateTime.Today.AddDays(20 - index)), now));
        }

        await db.SaveChangesAsync();

        var courses = await service.GetPublicCoursesAsync(new CourseQueryFilter());

        Assert.Equal(15, courses.Count);
        Assert.Equal(courses.OrderBy(item => item.CourseDate).Select(item => item.Id), courses.Select(item => item.Id));
    }

    [Fact]
    public async Task CourseDetail_HidesOptionalFullDescriptionAndWhatToExpectSections()
    {
        await using var factory = new TestWebApplicationFactory();
        const string slug = "skryte-sekce";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;
            var course = CreateCourse(slug, DateOnly.FromDateTime(DateTime.Today.AddDays(8)), now);
            course.FullDescription = "Tajný detailní popis";
            course.WhatToExpect = "Tajný průběh";
            course.IsFullDescriptionVisible = false;
            course.IsWhatToExpectVisible = false;
            db.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var response = await client.GetAsync($"/kurzy/{slug}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Tajný detailní popis", html);
        Assert.DoesNotContain("Tajný průběh", html);
        Assert.Contains("Přihlášení na kurz", html);
    }

    [Fact]
    public async Task CourseTextDefaults_AreEditablePerCourseTypeAndField()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ICourseService>();

        var defaults = await service.GetTextDefaultsAsync();
        Assert.Contains(defaults, item => item.Type == CourseType.Breathwork && item.Field == CourseTextField.ShortDescription);
        Assert.Contains(defaults, item => item.Type == CourseType.Horses && item.Field == CourseTextField.ShortDescription);

        var result = await service.UpdateTextDefaultAsync(CourseType.Horses, CourseTextField.ShortDescription, "Výchozí text pro koně");
        var updatedDefaults = await service.GetTextDefaultsAsync();

        Assert.True(result.Succeeded);
        Assert.Contains(updatedDefaults, item =>
            item.Type == CourseType.Horses &&
            item.Field == CourseTextField.ShortDescription &&
            item.Text == "Výchozí text pro koně");
    }

    [Fact]
    public async Task ImageUpload_SavesOriginalAndThumbnail_AndWarnsWhenSmall()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"vbodlaci-image-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var service = new CourseImageService(new FakeWebHostEnvironment(tempRoot));
            await using var stream = new MemoryStream();
            using (var generatedImage = new Image<Rgba32>(1, 1))
            {
                await generatedImage.SaveAsPngAsync(stream);
            }

            stream.Position = 0;
            var formFile = new FormFile(stream, 0, stream.Length, "Image", "small.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            var (result, image) = await service.SaveAsync(formFile);

            Assert.True(result.Succeeded);
            Assert.NotNull(image);
            Assert.NotNull(image!.Warning);
            Assert.True(File.Exists(Path.Combine(tempRoot, image.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
            Assert.True(File.Exists(Path.Combine(tempRoot, image.ThumbnailPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DraftDelete_RemovesUploadedImageFiles()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ICourseService>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var uploadRoot = Path.Combine(environment.WebRootPath, "uploads", "courses");
        Directory.CreateDirectory(uploadRoot);

        var imagePath = Path.Combine(uploadRoot, "delete-test.jpg");
        var thumbnailPath = Path.Combine(uploadRoot, "delete-test-thumb.jpg");
        await File.WriteAllTextAsync(imagePath, "image");
        await File.WriteAllTextAsync(thumbnailPath, "thumb");

        var now = DateTimeOffset.UtcNow;
        var course = CreateCourse("delete-images", DateOnly.FromDateTime(DateTime.Today.AddDays(3)), now);
        course.Status = CourseStatus.Draft;
        course.PublishedAt = null;
        course.ImagePath = "/uploads/courses/delete-test.jpg";
        course.ThumbnailPath = "/uploads/courses/delete-test-thumb.jpg";
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var result = await service.SoftDeleteAsync(course.Id);

        Assert.True(result.Succeeded);
        Assert.False(File.Exists(imagePath));
        Assert.False(File.Exists(thumbnailPath));
    }

    private static Course CreateCourse(string slug, DateOnly date, DateTimeOffset now)
    {
        return new Course
        {
            Id = Guid.NewGuid(),
            Type = CourseType.Breathwork,
            Status = CourseStatus.Published,
            Title = $"Kurz {slug}",
            Slug = slug,
            CourseDate = date,
            TimeText = "18:00-21:00",
            CityOrArea = "Hlinsko",
            PriceText = "1 900 Kč",
            ShortDescription = "Krátký popis",
            FullDescription = "Detailní popis",
            IsFullDescriptionVisible = true,
            WhatToExpect = "Co tě čeká",
            IsWhatToExpectVisible = true,
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        };
    }

    private sealed class FakeWebHostEnvironment(string webRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Vbodlaci.Web.Tests";

        public IFileProvider WebRootFileProvider { get; set; } = new PhysicalFileProvider(webRootPath);

        public string WebRootPath { get; set; } = webRootPath;

        public string EnvironmentName { get; set; } = "Development";

        public string ContentRootPath { get; set; } = webRootPath;

        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(webRootPath);
    }
}
