using System.Security.Claims;

namespace Core.Authentication;

public interface ICurrentUser
{
    Task<ClaimsPrincipal> GetPrincipalAsync();
}

public interface IUserAccess
{
    Task<CurrentUser> RequireUserAsync(CancellationToken cancellationToken = default);
    Task<CurrentUser> RequireAdminAsync(CancellationToken cancellationToken = default);
}

public sealed record CurrentUser(string Id, string Email, IReadOnlyList<string> Roles);
