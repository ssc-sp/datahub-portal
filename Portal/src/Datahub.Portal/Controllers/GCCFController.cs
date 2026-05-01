using Datahub.Core.Configuration;
using Datahub.Application.Authentication;
using Datahub.Portal.Pages;
using Datahub.Portal.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace Datahub.Portal.Controllers;

/// <summary>
/// Temporary controller to handle GCCF OIDC login and logout
/// The login method is only for testing
/// </summary>
[Route("/gccf")]
[FeatureGate(Features.GCCF_Feature)]
public class GCCFController(IConfiguration configuration) : Controller
{
    private readonly IConfiguration _configuration = configuration;

    private static string GetDefaultLoginReturnUrl(string locale)
    {
        return locale.StartsWith("fr", StringComparison.OrdinalIgnoreCase)
            ? PageRoutes.Home_FR
            : PageRoutes.Home;
    }

    private static string GetDefaultLogoutReturnUrl(string locale)
    {
        return locale.StartsWith("fr", StringComparison.OrdinalIgnoreCase)
            ? PageRoutes.Login_FR
            : PageRoutes.Login;
    }

    private bool IsDevAuthEnabled()
    {
        return !string.IsNullOrWhiteSpace(_configuration["GccfOidc:DevAuth:UserEmail"]);
    }

    private void SetDevAuthActive(bool active)
    {
        if (active)
        {
            Response.Cookies.Append(
                DevAuthHandler.ActiveCookieName,
                bool.TrueString,
                new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
        }
        else
        {
            Response.Cookies.Delete(
                DevAuthHandler.ActiveCookieName,
                new CookieOptions
                {
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });
        }
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login(string? returnUrl = null, string locale = "en-CA")
    {
        returnUrl ??= GetDefaultLoginReturnUrl(locale);

        if (IsDevAuthEnabled())
        {
            SetDevAuthActive(true);

            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return Redirect(PageRoutes.Home);
        }

        var devAuthResult = await HttpContext.AuthenticateAsync(DevAuthHandler.Scheme);
        if (devAuthResult.Succeeded && devAuthResult.Principal is not null)
        {
            await HttpContext.SignInAsync(ConfigureAuthenticationServices.GccfCookieScheme, devAuthResult.Principal);

            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return Redirect(PageRoutes.Home);
        }

        var props = new AuthenticationProperties { RedirectUri = returnUrl };
        // Pass the current UI culture as 'ui_locales' parameter so OIDC handler forwards it
        props.Parameters["ui_locales"] = locale;

        // This triggers the OIDC middleware to construct the URL and redirect
        return Challenge(props, ConfigureAuthenticationServices.GccfOidcScheme);
    }

    [HttpGet("logout")]
    [HttpGet("deconnexion")]
    public async Task<IActionResult> Logout(string? returnUrl = null, string locale = "en-CA")
    {
        returnUrl ??= GetDefaultLogoutReturnUrl(locale);

        if (IsDevAuthEnabled())
        {
            SetDevAuthActive(false);
            Response.Cookies.Delete(
                ConfigureAuthenticationServices.GccfCookieName,
                new CookieOptions
                {
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });

            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return Redirect(GetDefaultLogoutReturnUrl(locale));
        }

        // Prepare sign-out to clear the GCCF cookie and trigger OIDC end-session
        var props = new AuthenticationProperties { RedirectUri = returnUrl };
        // Ensure 'ui_locales' is forwarded to the OIDC end-session request
        props.Parameters["ui_locales"] = locale;

        var devAuthResult = await HttpContext.AuthenticateAsync(DevAuthHandler.Scheme);
        if (devAuthResult.Succeeded)
        {
            return SignOut(props, ConfigureAuthenticationServices.GccfCookieScheme);
        }

        // Sign out both the GCCF cookie and the GCCF OIDC session
        return SignOut(props, ConfigureAuthenticationServices.GccfCookieScheme, ConfigureAuthenticationServices.GccfOidcScheme);
    }

    [HttpGet("sector-identifier.json")]
    public IActionResult SectorIdentifier()
    {
        var host = Request.Host.ToUriComponent();
        var scheme = Request.Scheme;
        
        var redirectUris = new[]
        {
            $"{scheme}://{host}{ConfigureAuthenticationServices.GccfSigninURL}"
        };

        return Json(redirectUris);
    }
}