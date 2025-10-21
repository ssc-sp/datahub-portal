using Datahub.Portal.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Portal.Controllers;

[Route("/gccf")]
public class GCCFController : Controller
{
    [HttpGet("gccf-login")]
    public IActionResult GccfLogin(string? redirectUri)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri ?? "/"
        };
        return Challenge(properties, ConfigureAuthServices.GccfOidcScheme);
    }

    [HttpGet("gccf-logout")]
    public IActionResult GccfLogout(string? redirectUri)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri ?? "/"
        };
        return SignOut(properties, ConfigureAuthServices.GccfOidcScheme);
    }
}
