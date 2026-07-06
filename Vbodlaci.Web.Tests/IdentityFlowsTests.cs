using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vbodlaci.Web.Services.Email;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public sealed class IdentityFlowsTests
{
    private const string AdminEmail = "admin@vbodlaci.local";

    [Fact]
    public async Task RegisterRoute_IsNotAvailable()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Identity/Account/Register");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LoginPage_RendersCzechFormWithoutRegistrationOrExternalProviders()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Identity/Account/Login");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Přihlášení", content, StringComparison.Ordinal);
        Assert.Contains("Zapomenuté heslo", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Register", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("another service", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PasswordRecovery_FullFlow_AllowsLoginWithNewPassword()
    {
        var recorder = new RecordingEmailService();
        await using var baseFactory = new TestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailService>();
                services.AddSingleton<IEmailService>(recorder);
            });
        });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // request the reset link
        var forgotResponse = await client.PostAsync("/Identity/Account/ForgotPassword", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = AdminEmail
        }));
        var forgotBody = await forgotResponse.Content.ReadAsStringAsync();
        Assert.True(forgotResponse.StatusCode == HttpStatusCode.Redirect,
            $"status={forgotResponse.StatusCode} body={forgotBody[..Math.Min(600, forgotBody.Length)]}");

        var mail = Assert.Single(recorder.Sent);
        Assert.Equal(AdminEmail, mail.To);
        Assert.False(string.IsNullOrWhiteSpace(mail.TextBody));
        var linkMatch = Regex.Match(mail.TextBody!, @"https?://\S+");
        Assert.True(linkMatch.Success, "reset e-mail must contain a link");

        var resetUri = new Uri(linkMatch.Value);
        var code = System.Web.HttpUtility.ParseQueryString(resetUri.Query)["code"];
        Assert.False(string.IsNullOrWhiteSpace(code));

        // the linked page renders the reset form
        var resetPage = await client.GetAsync(resetUri.PathAndQuery);
        Assert.Equal(HttpStatusCode.OK, resetPage.StatusCode);

        // set the new password
        const string newPassword = "NoveHeslo12345";
        var resetResponse = await client.PostAsync("/Identity/Account/ResetPassword", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = AdminEmail,
            ["Input.Password"] = newPassword,
            ["Input.ConfirmPassword"] = newPassword,
            ["Input.Code"] = code!
        }));
        Assert.Equal(HttpStatusCode.Redirect, resetResponse.StatusCode);
        Assert.Contains("ResetPasswordConfirmation", resetResponse.Headers.Location!.OriginalString, StringComparison.OrdinalIgnoreCase);

        // login with the new password succeeds (redirect), old password fails (page re-render)
        var loginNew = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = AdminEmail,
            ["Input.Password"] = newPassword
        }));
        Assert.Equal(HttpStatusCode.Redirect, loginNew.StatusCode);

        var loginOld = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = AdminEmail,
            ["Input.Password"] = "Admin12345"
        }));
        Assert.Equal(HttpStatusCode.OK, loginOld.StatusCode);
    }

    private sealed class RecordingEmailService : IEmailService
    {
        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }
}
