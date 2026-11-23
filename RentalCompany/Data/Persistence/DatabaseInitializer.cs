using Data.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Data.Persistence;

internal sealed class DatabaseInitializer(IServiceScopeFactory scopeFactory, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RentalDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { IdentityRoles.Admin, IdentityRoles.User })
        {
            if (!await roles.RoleExistsAsync(name))
            {
                EnsureSuccess(await roles.CreateAsync(new IdentityRole(name)));
            }
        }

        if (!configuration.GetValue<bool>("DemoAccounts:Enabled"))
        {
            return;
        }

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedAccountAsync(users, "Admin", IdentityRoles.Admin);
        await SeedAccountAsync(users, "User", IdentityRoles.User);
    }

    private async Task SeedAccountAsync(UserManager<ApplicationUser> users, string section, string role)
    {
        var email = configuration[$"DemoAccounts:{section}:Email"];
        var password = configuration[$"DemoAccounts:{section}:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException($"Brak konfiguracji DemoAccounts:{section}:Email/Password.");
        }

        var user = await users.FindByEmailAsync(email);
        if (user is not null)
        {
            // Konta demonstracyjne mają wspólne hasła zapisane w konfiguracji projektu.
            if (!await users.CheckPasswordAsync(user, password))
            {
                var token = await users.GeneratePasswordResetTokenAsync(user);
                EnsureSuccess(await users.ResetPasswordAsync(user, token, password));
            }
            return;
        }

        user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        EnsureSuccess(await users.CreateAsync(user, password));
        EnsureSuccess(await users.AddToRoleAsync(user, role));
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Inicjalizacja Identity: " + string.Join(", ", result.Errors.Select(error => error.Code)));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
