using Datahub.Infrastructure.Services.UserManagement;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Portal.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public CultureController(ILogger<CultureController> logger)
    {
        _logger = logger;
    }

    public ILogger<CultureController> _logger { get; }

    public IActionResult SetCulture(string culture, string redirectionUri)
    {
        culture = UserCultureService.GetValidCulture(culture);
        HttpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));

        _logger.LogInformation($"New Culture = {culture} - Current Thread Culture = {Thread.CurrentThread.CurrentCulture.Name} - Redirect URL = {redirectionUri}");
        if (redirectionUri == null)
        {
            redirectionUri = "/";
        }
            
        return LocalRedirect(redirectionUri);

    }
}
