using Datahub.Core.Configuration;
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
        var props = new AuthenticationProperties { RedirectUri = returnUrl };
        // Pass the current UI culture as 'ui_locales' parameter
        props.Items["ui_locales"] = locale;

        // This triggers the OIDC middleware to construct the URL and redirect
        return Challenge(props, ConfigureAuthServices.GccfOidcScheme);
    }

    [HttpGet("sector-identifier.json")]
    public async Task<IActionResult> SectorIdentifier()
    {
        var host = Request.Host.ToUriComponent();
        var scheme = Request.Scheme;
        
        var redirectUris = new[]
        {
            $"{scheme}://{host}{ConfigureAuthServices.GccfSigninURL}"
        };

        return Json(redirectUris);
    }
}
