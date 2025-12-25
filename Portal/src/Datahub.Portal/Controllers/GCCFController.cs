using Datahub.Core.Configuration;
using Datahub.Portal.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Datahub.Portal.Controllers;

/// <summary>
/// Temporary controller to handle GCCF OIDC login and logout
/// The login method is only for testing
/// </summary>
[Route("/gccf")]
public class GCCFController(IFeatureManagerSnapshot featureManager) : Controller
{

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        // This triggers the OIDC middleware to construct the URL and redirect
        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl },
            ConfigureAuthServices.GccfOidcScheme);
    }

    [HttpGet("sector-identifier.json")]
    public async Task<IActionResult> SectorIdentifier()
    {
        if (!await featureManager.IsEnabledAsync(Features.GCCF_Feature))
        {
            return NotFound();
        }
        var host = Request.Host.ToUriComponent();
        var scheme = Request.Scheme;
        
        var redirectUris = new[]
        {
            $"{scheme}://{host}{ConfigureAuthServices.GccfSigninURL}"
        };

        return Json(redirectUris);
    }
}
