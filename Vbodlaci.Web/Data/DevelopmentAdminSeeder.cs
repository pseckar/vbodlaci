using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Security;

namespace Vbodlaci.Web.Data;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedConfiguredAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        var options = services.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            logger.LogWarning(
                "Admin seed was skipped because {Section} options are missing email or password.",
                AdminSeedOptions.SectionName);
            return;
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Could not create Admin role for development seed.");
            }
        }

        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var adminUser = await userManager.FindByEmailAsync(options.Email);
        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, options.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create configured admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Could not assign Admin role to development admin user.");
            }
        }
    }
}
