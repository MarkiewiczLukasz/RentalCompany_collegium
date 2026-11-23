using System.Security.Claims;
using Core.Authentication;
using Microsoft.AspNetCore.Components.Authorization;

namespace RentalCompany.Authentication;

internal sealed class BlazorCurrentUser(AuthenticationStateProvider authenticationState) : ICurrentUser
{
    public async Task<ClaimsPrincipal> GetPrincipalAsync()
        => (await authenticationState.GetAuthenticationStateAsync()).User;
}
