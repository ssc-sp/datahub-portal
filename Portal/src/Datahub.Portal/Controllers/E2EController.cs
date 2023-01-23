using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Datahub.Portal.Controllers
{
    [Route("[controller]")]
    [AllowAnonymous]
    public class E2EController : Controller
    {
        private readonly ILogger<ApiController> _logger;

        public E2EController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Headers["Authorization"].ToString();

            if (token == null)
                return Unauthorized();

            var claims = new List<Claim>()
            {
                new(ClaimTypes.Name, "iperez@apption.com"),
                new(ClaimTypes.Email, "iperez@apption.com"),
                new("http://schemas.microsoft.com/identity/claims/identityprovider", "https://sts.windows.net/cebd5e62-1ccb-4888-8f5f-5d1dda653395/"),
                //new("name", "Iroel Perez-Garcia"),
                new("http://schemas.microsoft.com/identity/claims/objectidentifier", "34a16480-1c78-4b65-933f-c6cce336346b"),
                new("preferred_username", "iperez@apption.com"),
                //new("rh", "0.AUYAk00ajCjYDk2TA_071hHIIggv2bSobHFNkP3WTGw6ta9GAG8."),
                //new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "VSL87l89lQVXpLFOwHqzk_vIIGjVCmACuAB9k-__ECM"),
                new("http://schemas.microsoft.com/identity/claims/tenantid", "8c1a4d93-d828-4d0e-9303-fd3bd611c822"),
                //new("uti", "eCD6N-pfLUSOEnVI7fg9AA"),
                //new("utid", "cebd5e62-1ccb-4888-8f5f-5d1dda653395"),
                //new("uid", "aca7c23b-9617-47c4-80dc-a59c95c22414"),
                new("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "default"),
                new("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "DEMO-Admin")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            _logger.LogDebug($"Unauthenticated hello world: {token}");

            return Redirect("/home");
        }
    }
}
