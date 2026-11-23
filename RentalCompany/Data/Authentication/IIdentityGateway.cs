using System.Security.Claims;

namespace Data.Authentication;

public interface IIdentityGateway
{
    Task<IdentityLogin?> AuthenticateAsync(string email, string password, TimeSpan lifetime, CancellationToken cancellationToken = default);
    Task<bool> IsSessionValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public sealed record IdentityLogin(ClaimsPrincipal Principal, DateTimeOffset ExpiresAt);

public static class IdentityRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
}
