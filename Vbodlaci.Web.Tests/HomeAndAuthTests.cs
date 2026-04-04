using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class HomeAndAuthTests
{
    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminCourses_WithoutLogin_RedirectsToLogin()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/admin/courses");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.OriginalString, StringComparison.OrdinalIgnoreCase);
    }
}
