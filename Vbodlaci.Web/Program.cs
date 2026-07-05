using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Configuration;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Contacts;
using Vbodlaci.Web.Application.Newsletter;
using Vbodlaci.Web.Application.Registrations;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Services.Contacts;
using Vbodlaci.Web.Services.Courses;
using Vbodlaci.Web.Services.Email;
using Vbodlaci.Web.Services.Newsletter;
using Vbodlaci.Web.Services.Registrations;
using Vbodlaci.Web.Services.Security;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseStaticWebAssets();
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".dpkeys")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<LegalIdentityOptions>(builder.Configuration.GetSection(LegalIdentityOptions.SectionName));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<NoopEmailService>();
builder.Services.AddScoped<SmtpEmailService>();
builder.Services.AddScoped<IEmailService>(serviceProvider =>
{
    var smtpOptions = serviceProvider.GetRequiredService<IOptions<SmtpOptions>>().Value;

    return smtpOptions.Enabled
        ? serviceProvider.GetRequiredService<SmtpEmailService>()
        : builder.Environment.IsDevelopment()
            ? serviceProvider.GetRequiredService<NoopEmailService>()
            : throw new InvalidOperationException("NoopEmailService is allowed only in Development.");
});

builder.Services.AddScoped<IEmailDispatcher, EmailDispatcher>();
builder.Services.AddScoped<IRateLimitService, InMemoryRateLimitService>();
builder.Services.AddScoped<ICourseImageService, CourseImageService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(AppRoles.Admin));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "RequireAdminRole");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Register", "RequireAdminRole");
});

var app = builder.Build();

await ApplyMigrationsAsync(app.Services);
await app.SeedConfiguredAdminAsync();

if (app.Environment.IsProduction())
{
    ValidateProductionReadiness(app.Services);
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    if (app.Environment.IsProduction())
    {
        app.UseHsts();
    }
}

app.UseForwardedHeaders();

if (app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();

static async Task ApplyMigrationsAsync(
    IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
}

static void ValidateProductionReadiness(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var smtpOptions = scope.ServiceProvider.GetRequiredService<IOptions<SmtpOptions>>().Value;
    var legalIdentity = scope.ServiceProvider.GetRequiredService<IOptions<LegalIdentityOptions>>().Value;

    if (!smtpOptions.Enabled || string.IsNullOrWhiteSpace(smtpOptions.Host) || string.IsNullOrWhiteSpace(smtpOptions.From))
    {
        throw new InvalidOperationException(
            "Production startup is blocked because SMTP is not fully configured.");
    }

    static bool IsPlaceholder(string value) => value.Contains("TODO", StringComparison.OrdinalIgnoreCase);

    if (IsPlaceholder(legalIdentity.BusinessName)
        || IsPlaceholder(legalIdentity.Address)
        || IsPlaceholder(legalIdentity.CompanyId)
        || IsPlaceholder(legalIdentity.ContactEmail)
        || IsPlaceholder(legalIdentity.ContactPhone))
    {
        throw new InvalidOperationException(
            "Production startup is blocked because LegalIdentity contains placeholder TODO values.");
    }
}
