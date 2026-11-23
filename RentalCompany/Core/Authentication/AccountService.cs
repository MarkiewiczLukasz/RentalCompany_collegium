using System.Net.Mail;
using System.Security.Claims;
using Data.Authentication;
using Microsoft.Extensions.Options;

namespace Core.Authentication;

internal sealed class AccountService(IIdentityGateway identity, IOptions<SessionOptions> options) : IAccountService
{
    private const string LoginError = "Nie udało się zalogować. Sprawdź dane lub spróbuj ponownie za kilka minut.";

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254 || !MailAddress.TryCreate(email.Trim(), out _)
            || string.IsNullOrEmpty(password) || password.Length > 1024)
        {
            return new LoginResult(null, null, LoginError);
        }

        var result = await identity.AuthenticateAsync(email.Trim(), password, TimeSpan.FromMinutes(options.Value.LifetimeMinutes), cancellationToken);
        return result is null
            ? new LoginResult(null, null, LoginError)
            : new LoginResult(result.Principal, result.ExpiresAt, null);
    }

    public Task<bool> IsSessionValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        => identity.IsSessionValidAsync(principal, cancellationToken);

    public Task LogoutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        => identity.RevokeSessionAsync(principal, cancellationToken);
}

public sealed class SessionOptions
{
    public int LifetimeMinutes { get; set; } = 480;
}
