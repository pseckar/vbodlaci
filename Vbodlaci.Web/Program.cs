using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Infrastructure.Courses;
using Vbodlaci.Web.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
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

builder.Services.AddScoped<ICourseRepository, EfCourseRepository>();
builder.Services.AddScoped<NoopEmailService>();
builder.Services.AddScoped<SmtpEmailService>();
builder.Services.AddScoped<IEmailService>(serviceProvider =>
{
    var smtpOptions = serviceProvider.GetRequiredService<IOptions<SmtpOptions>>().Value;
    return smtpOptions.Enabled
        ? serviceProvider.GetRequiredService<SmtpEmailService>()
        : serviceProvider.GetRequiredService<NoopEmailService>();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(AppRoles.Admin));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Register", "RequireAdminRole");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    await app.SeedDevelopmentAdminAsync();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public partial class Program;
