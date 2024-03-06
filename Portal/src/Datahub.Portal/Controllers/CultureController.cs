using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Portal.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public CultureController(ILogger<CultureController> logger)
    {
        Logger = logger;
    }

    public ILogger<CultureController> Logger { get; }

    public IActionResult SetCulture(string culture, string redirectionUri)
    {
        if (culture != null)
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));
        }

        Logger.LogInformation($"New Culture = {culture}");
        Logger.LogInformation($"Redirect URL = {redirectionUri}");
        Logger.LogInformation($"Current Thread Culture = {Thread.CurrentThread.CurrentCulture.Name}");
        if (redirectionUri == null)
        {
            redirectionUri = "/";
        }

        return LocalRedirect(redirectionUri);

    }
}