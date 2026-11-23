using System.Security.Claims;
using Data.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Data.Authentication;

internal sealed class IdentityGateway(IServiceScopeFactory scopeFactory) : IIdentityGateway
{
    private const string SessionIdClaim = "rental:session-id";

    public async Task<IdentityLogin?> AuthenticateAsync(string email, string password, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        // Scope na operację: UserManager i DbContext nie żyją przez cały obwód Blazor.
        await using var scope = scopeFactory.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signIn = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<RentalDbContext>();
        var user = await users.FindByEmailAsync(email);
        if (user is null || !(await signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true)).Succeeded)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var session = new UserSession { UserId = user.Id, CreatedAt = now, ExpiresAt = now.Add(lifetime) };
        var principal = await signIn.CreateUserPrincipalAsync(user);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(SessionIdClaim, session.Id));
        await db.UserSessions.Where(item => item.ExpiresAt <= now).ExecuteDeleteAsync(cancellationToken);
        db.UserSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return new IdentityLogin(principal, session.ExpiresAt);
    }

    public async Task<bool> IsSessionValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<RentalDbContext>();
        var claimsOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value.ClaimsIdentity;
        var userId = principal.FindFirstValue(claimsOptions.UserIdClaimType);
        var sessionId = principal.FindFirstValue(SessionIdClaim);
        if (userId is null || sessionId is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        if (!await db.UserSessions.AsNoTracking().AnyAsync(session => session.Id == sessionId && session.UserId == userId && session.ExpiresAt > now, cancellationToken))
        {
            return false;
        }

        var user = await users.FindByIdAsync(userId);
        if (user is null || await users.IsLockedOutAsync(user)
            || principal.FindFirstValue(claimsOptions.SecurityStampClaimType) != await users.GetSecurityStampAsync(user))
        {
            return false;
        }

        // Zmiana ról wymaga ponownego logowania, również w już otwartej karcie.
        var currentRoles = await users.GetRolesAsync(user);
        return currentRoles.ToHashSet(StringComparer.Ordinal)
            .SetEquals(principal.FindAll(claimsOptions.RoleClaimType).Select(claim => claim.Value));
    }

    public async Task RevokeSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var sessionId = principal.FindFirstValue(SessionIdClaim);
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (sessionId is null || userId is null)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RentalDbContext>();
        await db.UserSessions.Where(session => session.Id == sessionId && session.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }
}
