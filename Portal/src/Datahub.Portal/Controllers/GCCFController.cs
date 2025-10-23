using Datahub.Portal.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Portal.Controllers;

/// <summary>
/// Temporary controller to handle GCCF OIDC login and logout
/// The login method is only for testing
/// </summary>
[Route("/gccf")]
public class GCCFController : Controller
{
    [HttpGet("sector-identifier.json")]
    public IActionResult SectorIdentifier()
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
