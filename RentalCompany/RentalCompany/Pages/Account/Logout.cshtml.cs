using Core.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RentalCompany.Pages.Account;

[AllowAnonymous]
public sealed class LogoutModel(IAccountService accounts) : PageModel
{
    public IActionResult OnGet() => StatusCode(StatusCodes.Status405MethodNotAllowed);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await accounts.LogoutAsync(User, cancellationToken);
        await HttpContext.SignOutAsync(AuthenticationDefaults.CookieScheme);
        return LocalRedirect("/Account/Login");
    }
}
