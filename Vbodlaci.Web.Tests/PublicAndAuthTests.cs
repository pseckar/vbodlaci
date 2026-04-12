using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class PublicAndAuthTests
{
    [Fact]
    public async Task HomePage_ReturnsSuccess_AndContainsBrand()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("V bodláčí", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AdminIndex_WithoutLogin_RedirectsToLogin()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.OriginalString, StringComparison.OrdinalIgnoreCase);
    }
}
