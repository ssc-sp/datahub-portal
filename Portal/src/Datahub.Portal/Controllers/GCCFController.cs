using Datahub.Core.Configuration;
using Datahub.Application.Authentication;
using Datahub.Portal.Services.Auth;
using Microsoft.AspNetCore.Authentication;
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
public class GCCFController() : Controller
{

    [HttpGet("login")]
    public async Task<IActionResult> Login(string returnUrl = "/", string locale = "en-CA")
    {
        var devAuthResult = await HttpContext.AuthenticateAsync(DevAuthHandler.Scheme);
        if (devAuthResult.Succeeded && devAuthResult.Principal is not null)
        {
            await HttpContext.SignInAsync(ConfigureAuthenticationServices.GccfCookieScheme, devAuthResult.Principal);

            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return Redirect("/");
        }

        var props = new AuthenticationProperties { RedirectUri = returnUrl };
        // Pass the current UI culture as 'ui_locales' parameter so OIDC handler forwards it
        props.Parameters["ui_locales"] = locale;

        // This triggers the OIDC middleware to construct the URL and redirect
        return Challenge(props, ConfigureAuthenticationServices.GccfOidcScheme);
    }

    [HttpGet("logout")]
    [HttpGet("deconnexion")]
    public async Task<IActionResult> Logout(string returnUrl = "/", string locale = "en-CA")
    {
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