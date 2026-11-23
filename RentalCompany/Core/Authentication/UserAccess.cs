using System.Security.Claims;
using Data.Authentication;

namespace Core.Authentication;

internal sealed class UserAccess(ICurrentUser currentUser, IIdentityGateway identity) : IUserAccess
{
    public async Task<CurrentUser> RequireUserAsync(CancellationToken cancellationToken = default)
    {
        var principal = await currentUser.GetPrincipalAsync();
        if (!await identity.IsSessionValidAsync(principal, cancellationToken))
        {
            throw new UnauthorizedAccessException("Sesja wygasła. Wymagane ponowne logowanie.");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Brak tożsamości użytkownika.");
        }

        return new CurrentUser(userId, principal.FindFirstValue(ClaimTypes.Email) ?? principal.Identity?.Name ?? string.Empty,
            principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray());
    }

    public async Task<CurrentUser> RequireAdminAsync(CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(cancellationToken);
        if (!user.Roles.Contains(AppRoles.Admin))
        {
            throw new UnauthorizedAccessException("Operacja wymaga roli Admin.");
        }

        return user;
    }
}
