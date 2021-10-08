using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Datahub.Portal.Controllers
{
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
            if (culture != null)
            {
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));
            }

            _logger.LogInformation($"New Culture = {culture}");
            _logger.LogInformation($"Redirect URL = {redirectionUri}");
            _logger.LogInformation($"Current Thread Culture = {Thread.CurrentThread.CurrentCulture.Name}");
            if (redirectionUri == null) redirectionUri = "/";
            return LocalRedirect(redirectionUri);

        }
    }
}
