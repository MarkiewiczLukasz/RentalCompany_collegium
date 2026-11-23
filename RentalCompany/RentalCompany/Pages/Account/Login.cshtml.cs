using System.ComponentModel.DataAnnotations;
using Core.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RentalCompany.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel(IAccountService accounts) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Podaj adres e-mail."), EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail."), StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Podaj hasło."), StringLength(1024), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? LocalRedirect(SafeReturnUrl()) : Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await accounts.LoginAsync(Email, Password, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return Page();
        }

        await HttpContext.SignInAsync(AuthenticationDefaults.CookieScheme, result.Principal!, new AuthenticationProperties
        {
            IsPersistent = RememberMe,
            ExpiresUtc = result.ExpiresAt,
            AllowRefresh = false
        });
        return LocalRedirect(SafeReturnUrl());
    }

    private string SafeReturnUrl() => Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/";
}
