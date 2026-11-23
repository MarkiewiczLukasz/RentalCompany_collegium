using System.Security.Claims;

namespace Core.Authentication;

public interface IAccountService
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<bool> IsSessionValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task LogoutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public sealed record LoginResult(ClaimsPrincipal? Principal, DateTimeOffset? ExpiresAt, string? Error)
{
    public bool Succeeded => Principal is not null && ExpiresAt is not null;
}
