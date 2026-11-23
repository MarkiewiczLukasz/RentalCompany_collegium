using Core.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace RentalCompany.Authentication;

internal sealed class SessionAuthenticationStateProvider(ILoggerFactory loggerFactory, IAccountService accounts)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
        => accounts.IsSessionValidAsync(authenticationState.User, cancellationToken);
}
