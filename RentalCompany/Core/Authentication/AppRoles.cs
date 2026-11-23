using Data.Authentication;

namespace Core.Authentication;

public static class AppRoles
{
    public const string Admin = IdentityRoles.Admin;
    public const string User = IdentityRoles.User;
}

public static class AuthenticationDefaults
{
    public const string CookieScheme = "Identity.Application";
}
