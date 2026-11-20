using FitnessClub.Api.Auth;
using FitnessClub.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
namespace FitnessClub.Api.Data;
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var admin = services.GetRequiredService<IOptions<AdminUserSettings>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Створено роль {Role}", role);
            }
        }
        if (await userManager.FindByEmailAsync(admin.Email) is not null)
            return;
        var user = new AppUser
        {
            UserName = admin.Email,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(user, admin.Password);
        if (!result.Succeeded)
        {
            logger.LogError("Не вдалося створити адміністратора: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }
        await userManager.AddToRolesAsync(user, new[] { Roles.User, Roles.Administrator });
        logger.LogInformation("Створено адміністратора {Email}", admin.Email);
    }
}
