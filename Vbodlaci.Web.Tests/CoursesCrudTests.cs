using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Areas.Admin.Pages.Courses;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class CoursesCrudTests
{
    [Fact]
    public async Task CreatedCourse_IsVisibleInAdminList()
    {
        await using var factory = new TestWebApplicationFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ICourseRepository>();
            var now = DateTimeOffset.UtcNow;

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Title = "Testovací kurz vnitřní stability",
                Slug = "testovaci-kurz-vnitrni-stability",
                ShortDescription = "Krátký popis pro test.",
                Description = "Detailní popis pro testovací scénář.",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(21)),
                Capacity = 10,
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await repository.AddAsync(course);

            var pageModel = new IndexModel(repository)
            {
                PageContext = new PageContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = scope.ServiceProvider
                    }
                }
            };

            await pageModel.OnGetAsync();

            Assert.Contains(
                pageModel.Courses,
                item => item.Title.Equals("Testovací kurz vnitřní stability", StringComparison.Ordinal));
        }
    }
}
