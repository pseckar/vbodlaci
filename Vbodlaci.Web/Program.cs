using Microsoft.AspNetCore.DataProtection;
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
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing")) //TODO: why this environment? can be replaced with standard Staging? or removed altogether?
{
    builder.WebHost.UseStaticWebAssets();   //TODO: what does this mean, whats its purpose? How differently will it behave in production? 
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); //TODO: should be only for development env?
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".dpkeys")));//TODO: what is this good for?

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
    .AddDefaultTokenProviders() //TODO: tokens are stored in cookies or where?
    .AddDefaultUI(); //TODO: this should be reworked in future - simple custom UI instead

builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<LegalIdentityOptions>(builder.Configuration.GetSection(LegalIdentityOptions.SectionName));

builder.Services.AddMemoryCache();

builder.Services.AddScoped<NoopEmailService>();
builder.Services.AddScoped<SmtpEmailService>();
builder.Services.AddScoped<IEmailService>(serviceProvider =>
{
    var smtpOptions = serviceProvider.GetRequiredService<IOptions<SmtpOptions>>().Value;
    return smtpOptions.Enabled
        ? serviceProvider.GetRequiredService<SmtpEmailService>()
        : serviceProvider.GetRequiredService<NoopEmailService>();//TODO: should be only for development env? rework this, we should not allow using Noop in production
});

builder.Services.AddScoped<IEmailDispatcher, EmailDispatcher>();
builder.Services.AddScoped<IRateLimitService, InMemoryRateLimitService>();
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

await EnsureDatabaseAsync(app.Services);

var isDevLike = app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing");
if (isDevLike)
{
    app.UseMigrationsEndPoint();//TODO: what is this? DB migration? how will it behave in production?
    if (app.Environment.IsDevelopment())
    {
        await app.SeedDevelopmentAdminAsync();  //TODO: how will the admin be created in production?
    }
}
else if (app.Environment.IsProduction())
{
    ValidateLegalIdentity(app.Services);
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Error");
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

static async Task EnsureDatabaseAsync(
    IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

//TODO: Remove this when placeholder values are replaced with real legal identity information.
static void ValidateLegalIdentity(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var legalIdentity = scope.ServiceProvider.GetRequiredService<IOptions<LegalIdentityOptions>>().Value;

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

