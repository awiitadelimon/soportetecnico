using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SoporteTecnico.Infrastructure.Data;

namespace SoporteTecnico.Api;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var accounts = new[]
        {
            (
                Role: "Administrador",
                Email: configuration["SeedUsers:Administrator:Email"],
                Password: configuration["SeedUsers:Administrator:Password"]
            ),
            (
                Role: "Gestor",
                Email: configuration["SeedUsers:Gestor:Email"],
                Password: configuration["SeedUsers:Gestor:Password"]
            )
        };

        foreach (var account in accounts)
        {
            if (string.IsNullOrWhiteSpace(account.Email) ||
                string.IsNullOrWhiteSpace(account.Password))
            {
                throw new InvalidOperationException(
                    $"Falta configurar el correo o contraseña de {account.Role} en User Secrets.");
            }

            if (!await roleManager.RoleExistsAsync(account.Role))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(account.Role));

                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }

            var user = await userManager.FindByEmailAsync(account.Email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = account.Email,
                    Email = account.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    user, account.Password);

                if (!createResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, account.Role))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    user, account.Role);

                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
